using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Numerics;

namespace rendr_NET {
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct RendrColor {
		[FieldOffset(0)]
		public float Float;

		[FieldOffset(0)]
		public byte B;

		[FieldOffset(1)]
		public byte G;

		[FieldOffset(2)]
		public byte R;

		[FieldOffset(3)]
		public byte A;
	}

	public unsafe struct RendrBuffer {
		public RendrColor* Buffer;
		public int Width;
		public int Height;
	};

	public unsafe struct Triangle {
		public Vector3 A;
		public Vector3 B;
		public Vector3 C;

		public Vector2 A_UV;
		public Vector2 B_UV;
		public Vector2 C_UV;
	};

	public unsafe struct RndrVertex {
		public Vector3 Pos;
		public Vector2 UV;
	};

	public unsafe struct RndrVertexBuffer {
		public RndrVertex* Verts;
		public int Length;
	};


	public static unsafe class rendr_NET {
		const CallingConvention CConv = CallingConvention.Cdecl;

		public static Matrix4x4 Matrix4x4_Multiply(Matrix4x4 A, Matrix4x4 B) {
			return Matrix4x4.Multiply(A, B);
		}

		public static int abs(float V) {
			return (int)Math.Abs(V);
		}

		public static float sqrt(float V) {
			return (float)Math.Sqrt(V);
		}

		public static float max(float A, float B) {
			if (A > B)
				return A;

			return B;
		}

		public static float min(float A, float B) {
			if (A < B)
				return A;

			return B;
		}

		public static Vector3 Vec3(float X, float Y, float Z) {
			return new Vector3() { X = X, Y = Y, Z = Z };
		}

		public static Vector2 Vec2(float X, float Y) {
			return new Vector2() { X = X, Y = Y };
		}

		public static float Float_Vary(float A, float B, float C, Vector3 Bary) {
			return (A * Bary.X) + (B * Bary.Y) + (C * Bary.Z);
		}

		public static Vector3 Vector3_Sub(Vector3 A, Vector3 B) {
			return A - B;
		}

		public static Vector3 Vector3_Normalize(Vector3 V) {
			return Vector3.Normalize(V);
		}

		public static Vector3 Vector3_Cross(Vector3 A, Vector3 B) {
			return Vector3.Cross(A, B);
		}

		public static Vector3 Vector3_Transform(Vector3 V, Matrix4x4 Mat) {
			return Vector3.Transform(V, Mat);
		}

		public static float Min(float A, float B, float C) {
			return min(A, min(B, C));
		}

		public static float Max(float A, float B, float C) {
			return max(A, max(B, C));
		}

		public static float Clamp(float V, float Min, float Max) {
			if (V < Min)
				return Min;
			if (V > Max)
				return Max;
			return V;
		}

		public static void BoundingBox(Vector3 A, Vector3 B, Vector3 C, Vector3* Minimum, Vector3* Maximum) {
			*Minimum = Vec3(Min(A.X, B.X, C.X), Min(A.Y, B.Y, C.Y), Min(A.Z, B.Z, C.Z));
			*Maximum = Vec3(Max(A.X, B.X, C.X), Max(A.Y, B.Y, C.Y), Max(A.Z, B.Z, C.Z));
		}

		public static Vector3 Barycentric(Vector3 A, Vector3 B, Vector3 C, int PX, int PY) {
			Vector3 U = Vector3_Cross(Vec3(C.X - A.X, B.X - A.X, A.X - PX), Vec3(C.Y - A.Y, B.Y - A.Y, A.Y - PY));
			Vector3 Val = Vector3.Zero;

			if (abs(U.Z) < 1) {
				Val.X = -1;
				return Val;
			}

			Val.X = 1.0f - (U.X + U.Y) / U.Z;
			Val.Y = U.Y / U.Z;
			Val.Z = U.X / U.Z;
			return Val;
		}

		//-----------------------------------------------------------------------------------------

		public static RendrBuffer ColorBuffer;
		public static RendrBuffer DepthBuffer;
		public static RendrBuffer Tex0;

		public static RendrColor DrawColor;

		public static Matrix4x4 ViewMatrix;
		public static Matrix4x4 ProjectionMatrix;
		public static Matrix4x4 ModelMatrix;

		static bool WireframeEnabled = false;
		static bool EnableDepthTesting = true;
		static bool EnableTexturing = true;
		static bool EnableBackfaceCulling = true;


		[DllExport(CallingConvention = CConv)]
		public static RndrVertexBuffer* CreateVertexBuffer(Vector3* Points, Vector2* UVs, int Len) {
			RndrVertexBuffer* Ret = (RndrVertexBuffer*)Marshal.AllocHGlobal(sizeof(RndrVertexBuffer));

			if (Ret == null || Points == null || UVs == null || Len <= 0)
				return null;

			Ret->Verts = (RndrVertex*)Marshal.AllocHGlobal(sizeof(RndrVertex) * Len);


			if (Ret->Verts == null) {
				Marshal.FreeHGlobal((IntPtr)Ret);
				return null;
			}

			Ret->Length = Len;

			for (int i = 0; i < Len; i++) {
				Ret->Verts[i] = new RndrVertex() { Pos = Points[i], UV = UVs[i] };
			}

			return Ret;
		}

		[DllExport(CallingConvention = CConv)]
		public static void EnableWireframe(int Enable) {
			if (Enable != 0) {
				WireframeEnabled = true;
			} else {
				WireframeEnabled = false;
			}
		}


		[DllExport(CallingConvention = CConv)]
		public static void SetColorBuffer(RendrColor* Buffer, int Width, int Height) {
			ColorBuffer.Buffer = Buffer;
			ColorBuffer.Width = Width;
			ColorBuffer.Height = Height;
		}

		[DllExport(CallingConvention = CConv)]
		public static void SetDepthBuffer(RendrColor* Buffer, int Width, int Height) {
			DepthBuffer.Buffer = Buffer;
			DepthBuffer.Width = Width;
			DepthBuffer.Height = Height;
		}

		public static RendrColor IndexBuffer(RendrBuffer* Buffer, float U, float V) {
			int Height = Buffer->Height;
			int Width = Buffer->Width;

			return Buffer->Buffer[(int)(V * Height) * Width + (int)(U * Width)];
		}

		[DllExport(CallingConvention = CConv)]
		public static void SetTexBuffer(RendrColor* Buffer, int Width, int Height) {
			Tex0.Buffer = Buffer;
			Tex0.Width = Width;
			Tex0.Height = Height;
		}

		[DllExport(CallingConvention = CConv)]
		public static void SetDrawColor(byte R, byte G, byte B, byte A) {
			DrawColor.R = R;
			DrawColor.G = G;
			DrawColor.B = B;
			DrawColor.A = A;
		}

		[DllExport(CallingConvention = CConv)]
		public static void SetMatrix(Matrix4x4 Mat, int MatType) {
			switch (MatType) {
				case 0:
					ViewMatrix = Mat;
					break;

				case 1:
					ProjectionMatrix = Mat;
					break;

				case 2:
					ModelMatrix = Mat;
					break;
			}
		}

		[DllExport(CallingConvention = CConv)]
		public static void Clear(byte R, byte G, byte B, byte A, float Depth) {
			int Len = ColorBuffer.Width * ColorBuffer.Height;

			for (int i = 0; i < Len; i++) {
				ColorBuffer.Buffer[i].R = R;
				ColorBuffer.Buffer[i].G = G;
				ColorBuffer.Buffer[i].B = B;
				ColorBuffer.Buffer[i].A = A;

				if (EnableDepthTesting)
					DepthBuffer.Buffer[i].Float = Depth;
			}
		}

		// BUG: DllExport erases the exported method
		// therefore it's necessary to export a separate wrapper function if we want the original to be still invokable from inside the original C# code
		[DllExport(CallingConvention = CConv)]
		public static void DrawLine(int X0, int Y0, int X1, int Y1) {
			DrawLineImpl(X0, Y0, X1, Y1);
		}

		public static void DrawLineImpl(int X0, int Y0, int X1, int Y1) {
			bool Steep = false;

			if (abs(X0 - X1) < abs(Y0 - Y1)) {
				int Tmp = X0;
				X0 = Y0;
				Y0 = Tmp;

				Tmp = X1;
				X1 = Y1;
				Y1 = Tmp;

				Steep = true;
			}

			if (X0 > X1) {
				int Tmp = X0;
				X0 = X1;
				X1 = Tmp;

				Tmp = Y0;
				Y0 = Y1;
				Y1 = Tmp;
			}

			int DeltaX = X1 - X0;
			int DeltaY = Y1 - Y0;
			int DeltaError2 = abs(DeltaY) * 2;
			int Error2 = 0;
			int Y = Y0;

			for (int X = X0; X <= X1; X++) {
				if (Steep) {
					if (X < 0 || Y < 0 || Y >= ColorBuffer.Width || X >= ColorBuffer.Height)
						continue;

					ColorBuffer.Buffer[X * ColorBuffer.Width + Y] = DrawColor;
				} else {
					if (X < 0 || Y < 0 || X >= ColorBuffer.Width || Y >= ColorBuffer.Height)
						continue;

					ColorBuffer.Buffer[Y * ColorBuffer.Width + X] = DrawColor;
				}

				Error2 += DeltaError2;

				if (Error2 > DeltaX) {
					Y += (Y1 > Y0 ? 1 : -1);
					Error2 -= DeltaX * 2;
				}
			}
		}

		public static void Shader_Vertex(Vector3* V) {
			Matrix4x4 FinalMatrix = Matrix4x4_Multiply(Matrix4x4_Multiply(ModelMatrix, ViewMatrix), ProjectionMatrix);
			*V = Vector3_Transform(*V, FinalMatrix);
		}


		public static RendrColor Shader_Fragment(Vector3 Pos, Vector2 UV) {
			if (EnableTexturing) {
				fixed (RendrBuffer* Tex0Ptr = &Tex0)
					return IndexBuffer(Tex0Ptr, UV.X, UV.Y);
			}

			return DrawColor;
		}

		public static void DrawTriangle(RndrVertex* TriangleVerts, int Index) {
			RndrVertex A = TriangleVerts[Index];
			RndrVertex B = TriangleVerts[Index + 1];
			RndrVertex C = TriangleVerts[Index + 2];

			Shader_Vertex(&A.Pos);
			Shader_Vertex(&B.Pos);
			Shader_Vertex(&C.Pos);

			if (EnableBackfaceCulling) {
				Vector3 Cross = Vector3_Normalize(Vector3_Cross(Vector3_Sub(C.Pos, A.Pos), Vector3_Sub(B.Pos, A.Pos)));
				if (Cross.Z < 0)
					return;
			}

			if (WireframeEnabled) {
				DrawLineImpl((int)A.Pos.X, (int)A.Pos.Y, (int)B.Pos.X, (int)B.Pos.Y);
				DrawLineImpl((int)B.Pos.X, (int)B.Pos.Y, (int)C.Pos.X, (int)C.Pos.Y);
				DrawLineImpl((int)C.Pos.X, (int)C.Pos.Y, (int)A.Pos.X, (int)A.Pos.Y);
			} else {
				Vector3 Min;
				Vector3 Max;
				BoundingBox(A.Pos, B.Pos, C.Pos, &Min, &Max);

				for (int Y = (int)Min.Y; Y < Max.Y; Y++) {
					for (int X = (int)Min.X; X < Max.X; X++) {
						if (X < 0 || Y < 0 || X >= ColorBuffer.Width || Y >= ColorBuffer.Height)
							continue;

						Vector3 BCnt = Barycentric(A.Pos, B.Pos, C.Pos, X, Y);

						if (BCnt.X < 0 || BCnt.Y < 0 || BCnt.Z < 0)
							continue;

						int Idx = Y * ColorBuffer.Width + X;
						float D = Float_Vary(A.Pos.Z, B.Pos.Z, C.Pos.Z, BCnt);

						if (!EnableDepthTesting || (DepthBuffer.Buffer[Idx].Float > D)) {
							float TexU = Float_Vary(A.UV.X, B.UV.X, C.UV.X, BCnt);
							float TexV = Float_Vary(A.UV.Y, B.UV.Y, C.UV.Y, BCnt);
							RendrColor PixColor = Shader_Fragment(BCnt, Vec2(TexU, TexV));


							ColorBuffer.Buffer[Y * ColorBuffer.Width + X] = PixColor;

							if (EnableDepthTesting) {
								DepthBuffer.Buffer[Idx].Float = D;
							}
						}
					}
				}
			}
		}

		[DllExport(CallingConvention = CConv)]
		public static void DrawTriangles(RndrVertexBuffer* Vertices) {
			int Count = Vertices->Length / 3;

			for (int i = 0; i < Count; i++)
				DrawTriangle(Vertices->Verts, i * 3);
		}
	}
}
