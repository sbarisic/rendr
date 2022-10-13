using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Numerics;

namespace Launcher {
    public unsafe class rendr {
        const string Lib = "rendr_C.dll";
        const CallingConvention CConv = CallingConvention.Cdecl;


        [DllImport(Lib, CallingConvention = CConv)]
        public static extern void Init();

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
        public static extern void DrawLine(int X0, int Y0, int X1, int Y1);

        [DllImport(Lib, CallingConvention = CConv)]
        public static extern void DrawTriangle(void* Vertices, void* UVs, int Index);

        [DllImport(Lib, CallingConvention = CConv)]
        public static extern void DrawTriangles(void* Vertices, void* UVs, int Count);
    }
}
