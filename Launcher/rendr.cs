using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Launcher {
	public class rendr {
		const string Lib = "rendr_C.dll";
		const CallingConvention CConv = CallingConvention.Cdecl;


		[DllImport(Lib, CallingConvention = CConv)]
		public static extern void Init();
	}
}
