using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Launcher {
	static class Kernel32 {
		const string Kernel32Lib = "kernel32";

		[DllImport(Kernel32Lib, CharSet = CharSet.Unicode)]
		public static extern bool SetDllDirectory(string PathName);

		[DllImport(Kernel32Lib)]
		public static extern IntPtr LoadLibrary(string Lib);

		[DllImport(Kernel32Lib)]
		public static extern IntPtr GetProcAddress(IntPtr Module, string Proc);

		[DllImport(Kernel32Lib)]
		public static extern bool FreeLibrary(IntPtr Module);
	}
}
