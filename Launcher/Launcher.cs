using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;
using Glfw3;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

namespace Launcher {
    static class Kernel32 {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        public static extern bool SetDllDirectory(string PathName);
    }

    internal class Program {
        static void Main(string[] args) {
            bool Is64bit = IntPtr.Size == 8;
            string DllPath = Path.GetFullPath(".");

            if (Is64bit)
                DllPath = Path.Combine(DllPath, "x64");
            else
                DllPath = Path.Combine(DllPath, "x86");

            Kernel32.SetDllDirectory(DllPath);

            RendrProgram.Run();
        }
    }

    static unsafe class RendrProgram {
        static Glfw.Window Wind;
        static IntPtr Hwnd;
        static Graphics Gfx;

        const int W = 800;
        const int H = 800;


        public static void Run() {
            Console.WriteLine("Opening render window");

            if (!Glfw.Init()) {
                Console.WriteLine("Failed to initialize GLFW");
                Console.ReadLine();
                Environment.Exit(1);
            }

            Console.WriteLine("Using GLFW {0}", Glfw.GetVersionString());

            Glfw.WindowHint(Glfw.Hint.ClientApi, Glfw.ClientApi.None);
            Glfw.WindowHint(Glfw.Hint.Resizable, false);
            Wind = Glfw.CreateWindow(W, H, "Rendr");

            if (!Wind) {
                Console.WriteLine("Failed to create GLFW window");
                Glfw.Terminate();
                Environment.Exit(2);
            }

            Hwnd = Glfw.GetWin32Window(Wind);
            Gfx = Graphics.FromHwnd(Hwnd);
            CreateObjects();

            double LastTime = 0;
            float Dt = 0;

            float[] FPSArray = new float[100];
            int FPSIdx = 0;
            int PrintCounter = 0;

            while (!Glfw.WindowShouldClose(Wind)) {
                double CurTime = Glfw.GetTime();
                Dt = (float)(CurTime - LastTime);
                LastTime = CurTime;

                if (PrintCounter++ > 10) {
                    PrintCounter = 0;
                    Console.WriteLine("FPS = {0}", FPSArray.Sum() / FPSArray.Length);
                }

                FPSArray[FPSIdx++] = 1.0f / Dt;
                if (FPSIdx >= FPSArray.Length)
                    FPSIdx = 0;



                RenderLoop(Dt);
                Glfw.PollEvents();
            }

            Glfw.Terminate();
        }

        static DrawBitmap Framebuffer;
        static DrawBitmap DepthBuffer;
        static DrawBitmap Tex0;

        static Vector3[] Verts;
        static Vector2[] UVs;

        static Matrix4x4 ProjectionMatrix;
        static Matrix4x4 ViewMatrix;
        static Matrix4x4 ModelMatrix;

        static float Rad(float Deg) {
            return (float)(Deg * Math.PI / 180);
        }

        static void CreateObjects() {
            Framebuffer = new DrawBitmap(W, H);

            DepthBuffer = new DrawBitmap(W, H);
            DepthBuffer.Lock();
            rendr.SetDepthBuffer(DepthBuffer.Data, DepthBuffer.Width, DepthBuffer.Height);

            Tex0 = new DrawBitmap(Image.FromFile("models/cup_green/tex/cup_DefaultMaterial_AO.png"));
            Tex0.Lock();

            ObjLoader.Load("models/cup_green/cup_green_obj.obj", out Verts, out UVs);
            Console.WriteLine("Vertices: {0}", Verts.Length);

            Vector3 Center = Vector3.Zero;

            for (int i = 0; i < Verts.Length; i++) {
                Verts[i] *= 0.1f;
                Center += Verts[i];
            }

            Center = Center / Verts.Length;

            for (int i = 0; i < Verts.Length; i++) {
                Verts[i] -= Center / 2;
            }


            float S = Math.Min(W, H);
            ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(70 * (float)Math.PI / 180, (float)W / H, 1, 1000);
            ViewMatrix = Matrix4x4.CreateScale(new Vector3(1, -1, 1) * S / 2) * Matrix4x4.CreateTranslation(new Vector3(W / 2f, H / 2f, 500));
            ModelMatrix = Matrix4x4.CreateFromYawPitchRoll(Rad(220), Rad(45), 0);

            rendr.SetMatrix(ViewMatrix, 0);
            rendr.SetMatrix(ProjectionMatrix, 1);
            rendr.SetMatrix(ModelMatrix, 2);

            rendr.SetDrawColor(255, 255, 255, 255);
            rendr.SetTexBuffer(Tex0.Data, Tex0.Width, Tex0.Height);
        }

        static void RenderLoop(float Dt) {
            Framebuffer.Lock();

            rendr.SetColorBuffer(Framebuffer.Data, Framebuffer.Width, Framebuffer.Height);
            rendr.Clear(0, 0, 0, 255, 0.0f);

            fixed (Vector3* VertsPtr = Verts)
            fixed (Vector2* UVsPtr = UVs) {
                rendr.DrawTriangles(VertsPtr, UVsPtr, Verts.Length / 3);
            }

            Framebuffer.Unlock();
            Gfx.DrawImageUnscaled(Framebuffer.Bmp, 0, 0, W, H);
        }
    }

    unsafe class DrawBitmap {
        public Bitmap Bmp;

        public int Width;
        public int Height;

        public void* Data;
        public int Stride;

        BitmapData BmpData;

        public DrawBitmap(int W, int H) {
            Bmp = new Bitmap(W, H);

            this.Width = W;
            this.Height = H;
        }

        public DrawBitmap(Image Img) {
            Img.RotateFlip(RotateFlipType.RotateNoneFlipY);
            Bmp = new Bitmap(Img);

            this.Width = Img.Width;
            this.Height = Img.Height;
        }

        public void Lock() {
            BmpData = Bmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            Data = (void*)BmpData.Scan0;
            Stride = BmpData.Stride;
        }

        public void Unlock() {
            Bmp.UnlockBits(BmpData);
        }
    }
}
