using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using System.Reflection;

namespace Launcher {
	public unsafe delegate void BufferFunc(void* Buffer, int Width, int Height);

	public unsafe delegate void VoidPtrFunc(void* Ptr);

	public unsafe delegate void* CreateVertexBufferFunc(void* Points, void* UVs, int Len);

	public delegate void RGBAFunc(byte R, byte G, byte B, byte A);

	public delegate void RGBADFunc(byte R, byte G, byte B, byte A, float D);

	public delegate void MatrixFunc(Matrix4x4 Mat, int MatType);

	[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	sealed class DllImport2Attribute : Attribute {
		public DllImport2Attribute() {
		}
	}


	public unsafe static class rendr {
		static IntPtr Module;

		const string Lib = "rendr_NET.dll";
		// const string Lib = "rendr_C.dll";
		// const CallingConvention CConv = CallingConvention.Cdecl;

		[DllImport2]
		public static BufferFunc SetColorBuffer;

		[DllImport2]
		public static BufferFunc SetDepthBuffer;

		[DllImport2]
		public static BufferFunc SetTexBuffer;

		[DllImport2]
		public static RGBAFunc SetDrawColor;

		[DllImport2]
		public static MatrixFunc SetMatrix;

		[DllImport2]
		public static RGBADFunc Clear;

		[DllImport2]
		public static VoidPtrFunc DrawTriangles;

		[DllImport2]
		public static CreateVertexBufferFunc CreateVertexBuffer;

		/*
		[DllImport(Lib, CallingConvention = CConv)]
		public static extern void SetColorBuffer(void* Buffer, int Width, int Height);

		[DllImport(Lib, CallingConvention = CConv)]
		public static extern void SetDepthBuffer(void* Buffer, int Width, int Height);

		[DllImport(Lib, CallingConvention = CConv)]
		public static extern void SetTexBuffer(void* Buffer, int Width, int Height);
		 
		[DllImport(Lib, CallingConvention = CConv)]
		public static extern void SetDrawColor(byte R, byte G, byte B, byte A);

		[DllImport(Lib, CallingConvention = CConv)]
		public static extern void SetMatrix(Matrix4x4 Mat, int MatType);

		[DllImport(Lib, CallingConvention = CConv)]
		public static extern void Clear(byte R, byte G, byte B, byte A, float Depth);

		[DllImport(Lib, CallingConvention = CConv)]
		public static extern void DrawTriangles(void* Vertices);

		[DllImport(Lib, CallingConvention = CConv)]
		public static extern void* CreateVertexBuffer(void* Points, void* UVs, int Len);*/


		public static void BindLibrary(string LibName) {
			Module = Kernel32.LoadLibrary(LibName);

			if (Module == IntPtr.Zero)
				throw new Exception("Could not LoadLibrary: " + LibName);

			FieldInfo[] Fields = typeof(rendr).GetFields(BindingFlags.Public | BindingFlags.Static);
			for (int i = 0; i < Fields.Length; i++) {
				DllImport2Attribute DllImport2 = Fields[i].GetCustomAttribute<DllImport2Attribute>();

				if (DllImport2 == null)
					continue;

				Type DelegateType = Fields[i].FieldType;
				IntPtr ProcAddress = Kernel32.GetProcAddress(Module, Fields[i].Name);

				if (ProcAddress == null)
					throw new Exception("Could not GetProcAddress: " + Fields[i].Name);

				Delegate Del = Marshal.GetDelegateForFunctionPointer(ProcAddress, DelegateType);
				Fields[i].SetValue(null, Del);
			}
		}
	}
}
