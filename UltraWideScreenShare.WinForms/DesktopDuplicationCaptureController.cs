using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpGen.Runtime;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace UltraWideScreenShare.WinForms
{
    internal sealed class DesktopDuplicationCaptureController : IDisposable
    {
        private readonly Control _hostControl;
        private readonly Func<Rectangle> _regionProvider;
        private readonly object _frameLock = new();

        private IDXGIFactory2? _dxgiFactory;
        private ID3D11Device? _device;
        private ID3D11DeviceContext? _context;
        private ID3D11Multithread? _multiThread;
        private IDXGIOutputDuplication? _duplication;
        private IDXGISwapChain1? _swapChain;
        private Rectangle _monitorBounds;
        private Size _currentRegionSize;
        private IntPtr _monitorHandle;

        // Cursor support removed - see TASKS.md for future implementation

        private const int DxgiErrorWaitTimeout = unchecked((int)0x887A0027);
        private const int DxgiErrorAccessLost = unchecked((int)0x887A0026);

        public DesktopDuplicationCaptureController(Control hostControl, Func<Rectangle> regionProvider)
        {
            _hostControl = hostControl ?? throw new ArgumentNullException(nameof(hostControl));
            _regionProvider = regionProvider ?? throw new ArgumentNullException(nameof(regionProvider));
        }

        public void Start(IntPtr monitorHandle, Rectangle monitorBounds)
        {
            _monitorHandle = monitorHandle;
            _monitorBounds = monitorBounds;

            InitializeDevice();
            InitializeDuplication(monitorHandle);
            EnsureSwapChain(_monitorBounds.Size);
        }

        public void ProcessFrame()
        {
            if (_duplication == null || _context == null || _swapChain == null)
            {
                return;
            }

            IDXGIResource? frameResource = null;
            bool frameAcquired = false;

            try
            {
                Result result = _duplication.AcquireNextFrame(0, out _, out frameResource);
                if (result.Code == DxgiErrorWaitTimeout)
                {
                    return;
                }

                if (result.Code == DxgiErrorAccessLost)
                {
                    ReinitializeDuplication();
                    return;
                }

                result.CheckError();
                frameAcquired = true;

                using var frameTexture = frameResource!.QueryInterface<ID3D11Texture2D>();
                PresentRegion(frameTexture);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"desktop_duplication_frame_failed: {ex}");
                ReinitializeDuplication();
            }
            finally
            {
                frameResource?.Dispose();

                if (frameAcquired)
                {
                    try
                    {
                        _duplication.ReleaseFrame();
                    }
                    catch
                    {
                        ReinitializeDuplication();
                    }
                }
            }
        }

        private void InitializeDevice()
        {
            if (_device != null)
            {
                return;
            }

            var flags = DeviceCreationFlags.BgraSupport | DeviceCreationFlags.VideoSupport;
            var featureLevels = new[]
            {
                FeatureLevel.Level_11_1,
                FeatureLevel.Level_11_0
            };

            D3D11.D3D11CreateDevice(
                null,
                DriverType.Hardware,
                flags,
                featureLevels,
                out _device,
                out FeatureLevel _,
                out _context).CheckError();

            _multiThread = _device!.QueryInterfaceOrNull<ID3D11Multithread>();
            _multiThread?.SetMultithreadProtected(true);

            _dxgiFactory = DXGI.CreateDXGIFactory2<IDXGIFactory2>(debug: false);
        }

        private void InitializeDuplication(IntPtr monitorHandle)
        {
            DisposeDuplication();

            if (_device == null || _dxgiFactory == null)
            {
                throw new InvalidOperationException("Device not initialized.");
            }

            for (int adapterIndex = 0;; adapterIndex++)
            {
                var adapterResult = _dxgiFactory.EnumAdapters1(adapterIndex, out IDXGIAdapter1? adapter);
                if (adapterResult.Failure)
                {
                    break;
                }

                using (adapter)
                {
                    for (int outputIndex = 0;; outputIndex++)
                    {
                        var outputResult = adapter.EnumOutputs(outputIndex, out IDXGIOutput? output);
                        if (outputResult.Failure)
                        {
                            break;
                        }

                        var desc = output.Description;
                        if (desc.Monitor == monitorHandle)
                        {
                            using var output1 = output.QueryInterface<IDXGIOutput1>();
                            _duplication = output1.DuplicateOutput(_device);
                            return;
                        }

                        output.Dispose();
                    }
                }
            }

            throw new InvalidOperationException("Unable to create desktop duplication for the selected monitor.");
        }

        private void ReinitializeDuplication()
        {
            if (_duplication == null)
            {
                return;
            }

            try
            {
                InitializeDuplication(_monitorHandle);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"desktop_duplication_reinitialize_failed: {ex}");
            }
        }

        private void PresentRegion(ID3D11Texture2D frameTexture)
        {
            if (_swapChain == null || _context == null)
            {
                return;
            }

            var targetRegion = _regionProvider();
            var intersected = Rectangle.Intersect(targetRegion, _monitorBounds);
            if (intersected.Width <= 0 || intersected.Height <= 0)
            {
                return;
            }

            if (_currentRegionSize != intersected.Size)
            {
                EnsureSwapChain(intersected.Size);
            }

            int offsetX = intersected.Left - _monitorBounds.Left;
            int offsetY = intersected.Top - _monitorBounds.Top;

            using var backBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0);
            var region = new Box(offsetX, offsetY, 0, offsetX + intersected.Width, offsetY + intersected.Height, 1);

            _context.CopySubresourceRegion(backBuffer, 0, 0, 0, 0, frameTexture, 0, region);

            // No cursor compositing - see TASKS.md for future cursor implementation

            _swapChain.Present(1, PresentFlags.None);
        }

        private void EnsureSwapChain(Size targetSize)
        {
            if (_device == null || _dxgiFactory == null)
            {
                return;
            }

            targetSize = new Size(Math.Max(1, targetSize.Width), Math.Max(1, targetSize.Height));

            if (_swapChain == null)
            {
                var desc = new SwapChainDescription1
                {
                    Width = targetSize.Width,
                    Height = targetSize.Height,
                    Format = Format.B8G8R8A8_UNorm,
                    Stereo = false,
                    SampleDescription = new SampleDescription(1, 0),
                    BufferUsage = Usage.RenderTargetOutput,
                    BufferCount = 2,
                    Scaling = Scaling.None,
                    SwapEffect = SwapEffect.FlipSequential,
                    AlphaMode = AlphaMode.Ignore,
                    Flags = SwapChainFlags.None
                };

                _swapChain = _dxgiFactory.CreateSwapChainForHwnd(_device, _hostControl.Handle, desc, null, null);

                var windowHandle = _hostControl.FindForm()?.Handle ?? IntPtr.Zero;
                if (windowHandle != IntPtr.Zero)
                {
                    _dxgiFactory.MakeWindowAssociation(windowHandle, WindowAssociationFlags.IgnoreAltEnter);
                }
            }
            else if (_currentRegionSize != targetSize)
            {
                _context?.ClearState();
                _swapChain.ResizeBuffers(0, targetSize.Width, targetSize.Height, Format.B8G8R8A8_UNorm, SwapChainFlags.None);
            }

            _currentRegionSize = targetSize;
        }

        // Cursor-related methods removed - see TASKS.md for future implementation

        private void DisposeDuplication()
        {
            _duplication?.Dispose();
            _duplication = null;
        }

        public void Dispose()
        {
            DisposeDuplication();
            _swapChain?.Dispose();
            _swapChain = null;
            _multiThread?.Dispose();
            _multiThread = null;
            _context?.Dispose();
            _context = null;
            _device?.Dispose();
            _device = null;
            _dxgiFactory?.Dispose();
            _dxgiFactory = null;
        }
    }
}
