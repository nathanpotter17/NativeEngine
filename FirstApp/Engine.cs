
using Silk.NET.Maths;
using Silk.NET.WebGPU;
using Silk.NET.Windowing;
using System.Runtime.InteropServices;

namespace FirstApp
{
    public unsafe class Engine : IDisposable
    {
        private IWindow window;
        private WebGPU wgpu;
        private Instance* instance;
        private Surface* surface;
        private Adapter* adapter;
        private Device* device;

        private Queue* queue;
        private CommandEncoder* currentCommandEncoder;
        private RenderPassEncoder* currentRenderPassEncoder;
        private SurfaceTexture surfaceTexture;
        private TextureView* surfaceTextureView;
        public void Initialize()
        {
         
            WindowOptions windowOptions = new WindowOptions();
            windowOptions.Size = new Vector2D<int>(1280, 720);
            windowOptions.Title = "New WGPU Window";
            windowOptions.API = GraphicsAPI.None;

            window = Window.Create(windowOptions);

            window.Initialize();

            CreateAPI();
            CreateInstance();
            CreateSurface();
            CreateAdapter();
            CreateDevice();
            ConfigureSurface();

            window.Load += OnLoad;
            window.Update += OnUpdate;
            window.Render += OnRender;
            window.Run();
        }

        private void ConfigureSurface()
        {
            SurfaceConfiguration surfaceConfiguration = new SurfaceConfiguration();
            surfaceConfiguration.Device = device;
            surfaceConfiguration.Width = (uint)window.Size.X;
            surfaceConfiguration.Height = (uint)window.Size.Y;
            surfaceConfiguration.Format = TextureFormat.Bgra8Unorm;
            surfaceConfiguration.PresentMode = PresentMode.Fifo;
            surfaceConfiguration.Usage = TextureUsage.RenderAttachment;
            wgpu.SurfaceConfigure(surface, surfaceConfiguration);
            Console.WriteLine("Surface Configured.");
        }

        private void Configure()
        {
            PfnErrorCallback callback = PfnErrorCallback.From(
                (type, msgPtr, userDataPtr) => 
            {
                    string msg = Marshal.PtrToStringAnsi((IntPtr)msgPtr);
                    Console.WriteLine($"Error internal: {msg}");
            });

            wgpu.DeviceSetUncapturedErrorCallback(device, callback, null);
        }

        private void CreateAPI()
        {
            wgpu = WebGPU.GetApi();
            Console.WriteLine("Created API");
        }

        private void CreateInstance()
        {
            InstanceDescriptor descriptor = new InstanceDescriptor();
            instance = wgpu.CreateInstance(descriptor);
            Console.WriteLine("Created Instance");
        }

        private void CreateSurface()
        {
            surface = window.CreateWebGPUSurface(wgpu, instance);
            Console.WriteLine("Created Surface");
        }

        private void CreateAdapter()
        {
            RequestAdapterOptions options = new RequestAdapterOptions();

            options.CompatibleSurface = surface;
            options.BackendType = BackendType.D3D12;
            options.PowerPreference = PowerPreference.HighPerformance;

            PfnRequestAdapterCallback callback = PfnRequestAdapterCallback.From((status, wgpuAdapater, msgPtr, userDataPtr) => {
                if(status == RequestAdapterStatus.Success)
                {
                    this.adapter = wgpuAdapater;
                    Console.WriteLine("Created Adapter");
                }
                else
                {
                    string msg = Marshal.PtrToStringAnsi((IntPtr)msgPtr);
                    Console.WriteLine($"Error Retrieving Context: {msg}");
                }
            });
            wgpu.InstanceRequestAdapter(instance, options, callback, null);
        }

        private void CreateDevice()
        {
            PfnRequestDeviceCallback callback = PfnRequestDeviceCallback.From((status, wgpuDevice, msgPtr, userDataPtr) => {
                if (status == RequestDeviceStatus.Success)
                {
                    this.device = wgpuDevice;
                    Console.WriteLine("Created Device");
                }
                else
                {
                    string msg = Marshal.PtrToStringAnsi((IntPtr)msgPtr);
                    Console.WriteLine($"Error Retrieving Device: {msg}");
                }
            });

            DeviceDescriptor descriptor = new DeviceDescriptor();

            wgpu.AdapterRequestDevice(adapter, descriptor, callback, null);
        }

        private void OnLoad()
        {
        
        }
        private void OnUpdate(double dt)
        {
        
        }
        private void OnRender(double dt)
        {
            BeforeRender();

            // Draw in here

            AfterRender();
        }

        private void BeforeRender()
        {   
            // Render Queue
            queue = wgpu.DeviceGetQueue(device);

            // Command Encoder
            currentCommandEncoder = wgpu.DeviceCreateCommandEncoder(device, null);

            // Surface Texture
            wgpu.SurfaceGetCurrentTexture(surface, ref surfaceTexture);
            surfaceTextureView = wgpu.TextureCreateView(surfaceTexture.Texture, null);

            // Render Pass Encoder
            RenderPassColorAttachment* colorAttatchments = stackalloc RenderPassColorAttachment[1];
            colorAttatchments[0].View = surfaceTextureView;
            colorAttatchments[0].LoadOp = LoadOp.Clear;
            colorAttatchments[0].ClearValue = new Color(0.1, 0.9, 0.9, 1.0);
            colorAttatchments[0].StoreOp = StoreOp.Store;

            RenderPassDescriptor renderPassDescriptor = new RenderPassDescriptor();
            renderPassDescriptor.ColorAttachments = colorAttatchments;
            renderPassDescriptor.ColorAttachmentCount = 1;

            currentRenderPassEncoder = wgpu.CommandEncoderBeginRenderPass(currentCommandEncoder, renderPassDescriptor);
        }

        private void AfterRender()
        {   
            // Finish pass
            wgpu.RenderPassEncoderEnd(currentRenderPassEncoder);

            // Finish with command encoder
            CommandBuffer* commandBuffer = wgpu.CommandEncoderFinish(currentCommandEncoder, null);

            // Put encoder in queue (can take an array of pointers, or pointer-to-pointer)
            wgpu.QueueSubmit(queue, 1, &commandBuffer);

            wgpu.SurfacePresent(surface);

            // Dispose
            wgpu.TextureViewRelease(surfaceTextureView);
            wgpu.RenderPassEncoderRelease(currentRenderPassEncoder);
            wgpu.CommandBufferRelease(commandBuffer);
            wgpu.CommandEncoderRelease(currentCommandEncoder);
        }

        public void Dispose()
        {
            wgpu.DeviceDestroy(device);
            Console.WriteLine("WGPU Device Shut Down.");
            wgpu.SurfaceRelease(surface);
            Console.WriteLine("WGPU Surface Released");
            wgpu.AdapterRelease(adapter);
            Console.WriteLine("WGPU Adapter Released");
            wgpu.InstanceRelease(instance);
            Console.WriteLine("WGPU Instance Released");

        }

    }
}
