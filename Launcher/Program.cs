using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

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

			Console.WriteLine("Done!");
			Console.ReadLine();
		}
	}
}
