using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace rendr_NET {
	[StructLayout(LayoutKind.Sequential)]
	struct RendrColor {
		public byte B;
		public byte G;
		public byte R;
		public byte A;

		public RendrColor(byte R, byte G, byte B, byte A) {
			this.R = R;
			this.G = G;
			this.B = B;
			this.A = A;
		}
	}

	public static class rendr_NET {
		const CallingConvention CConv = CallingConvention.Cdecl;

		[DllExport(CallingConvention = CConv)]
		public static void Init() {
			Console.WriteLine("rendr from .NET");
		}
	}
}
