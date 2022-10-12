using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glfw3;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Launcher {
	static unsafe class RendrProgram {
		static Glfw.Window Wind;
		static IntPtr Hwnd;
		static Graphics Gfx;

		const int W = 800;
		const int H = 600;

		static DrawBitmap Framebuffer;

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

			Stopwatch SWatch = Stopwatch.StartNew();
			float Dt = 0;

			while (!Glfw.WindowShouldClose(Wind)) {
				// Cap framerate to 100 FPS
				while (SWatch.ElapsedMilliseconds < 10)
					;

				Dt = SWatch.ElapsedMilliseconds / 1000.0f;
				SWatch.Restart();

				RenderLoop(Dt);
				Glfw.PollEvents();
			}

			Glfw.Terminate();
		}

		static void CreateObjects() {
			Framebuffer = new DrawBitmap(W, H);


			Triangles = ObjLoader.Load("models\\diablo3_pose\\diablo3_pose.obj");
		}

		static void RenderLoop(float Dt) {
			Framebuffer.Lock();

			rendr.SetColorBuffer(Framebuffer.Data, Framebuffer.Width, Framebuffer.Height);
			rendr.Fill(0, 0, 0, 255);

			rendr.SetDrawColor(255, 255, 255, 255);
			rendr.Line(10, 20, 300, 200);

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
