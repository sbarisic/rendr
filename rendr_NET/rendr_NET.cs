﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace rendr_NET {
	public static class rendr_NET {
		const CallingConvention CConv = CallingConvention.Cdecl;

		[DllExport(CallingConvention = CConv)]
		public static void Init() {
			Console.WriteLine("rendr from .NET");
		}
	}
}
