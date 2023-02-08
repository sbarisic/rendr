#include <math.h>
#include <stdbool.h>
#include <stdio.h>
#include <stdlib.h>

#define EXPORT __declspec(dllexport)

typedef unsigned char byte;

typedef struct {
	union {
		struct {
			byte B;
			byte G;
			byte R;
			byte A;
		};

		float Float;
	};

} RendrColor;

typedef struct {
	RendrColor* Buffer;
	int Width;
	int Height;
} RendrBuffer;

typedef struct {
	float X;
	float Y;
	float Z;
} Vector3;

typedef struct {
	float X;
	float Y;
} Vector2;

typedef struct {
	Vector3 A;
	Vector3 B;
	Vector3 C;

	Vector2 A_UV;
	Vector2 B_UV;
	Vector2 C_UV;
} Triangle;

typedef struct {
	float M11;
	float M12;
	float M13;
	float M14;
	float M21;
	float M22;
	float M23;
	float M24;
	float M31;
	float M32;
	float M33;
	float M34;
	float M41;
	float M42;
	float M43;
	float M44;
} Matrix4x4;

typedef struct {
	Vector3 Pos;
	Vector2 UV;
} RndrVertex;

typedef struct {
	RndrVertex* Verts;
	int Length;
} RndrVertexBuffer;

//-----------------------------------------------------------------------------------------
//------------ Generic Utility Functions --------------------------------------------------
//-----------------------------------------------------------------------------------------

inline Matrix4x4 Matrix4x4_Multiply(Matrix4x4 A, Matrix4x4 B) {
	Matrix4x4 Res = { 0 };
	Res.M11 = A.M11 * B.M11 + A.M12 * B.M21 + A.M13 * B.M31 + A.M14 * B.M41;
	Res.M12 = A.M11 * B.M12 + A.M12 * B.M22 + A.M13 * B.M32 + A.M14 * B.M42;
	Res.M13 = A.M11 * B.M13 + A.M12 * B.M23 + A.M13 * B.M33 + A.M14 * B.M43;
	Res.M14 = A.M11 * B.M14 + A.M12 * B.M24 + A.M13 * B.M34 + A.M14 * B.M44;
	Res.M21 = A.M21 * B.M11 + A.M22 * B.M21 + A.M23 * B.M31 + A.M24 * B.M41;
	Res.M22 = A.M21 * B.M12 + A.M22 * B.M22 + A.M23 * B.M32 + A.M24 * B.M42;
	Res.M23 = A.M21 * B.M13 + A.M22 * B.M23 + A.M23 * B.M33 + A.M24 * B.M43;
	Res.M24 = A.M21 * B.M14 + A.M22 * B.M24 + A.M23 * B.M34 + A.M24 * B.M44;
	Res.M31 = A.M31 * B.M11 + A.M32 * B.M21 + A.M33 * B.M31 + A.M34 * B.M41;
	Res.M32 = A.M31 * B.M12 + A.M32 * B.M22 + A.M33 * B.M32 + A.M34 * B.M42;
	Res.M33 = A.M31 * B.M13 + A.M32 * B.M23 + A.M33 * B.M33 + A.M34 * B.M43;
	Res.M34 = A.M31 * B.M14 + A.M32 * B.M24 + A.M33 * B.M34 + A.M34 * B.M44;
	Res.M41 = A.M41 * B.M11 + A.M42 * B.M21 + A.M43 * B.M31 + A.M44 * B.M41;
	Res.M42 = A.M41 * B.M12 + A.M42 * B.M22 + A.M43 * B.M32 + A.M44 * B.M42;
	Res.M43 = A.M41 * B.M13 + A.M42 * B.M23 + A.M43 * B.M33 + A.M44 * B.M43;
	Res.M44 = A.M41 * B.M14 + A.M42 * B.M24 + A.M43 * B.M34 + A.M44 * B.M44;
	return Res;
}



inline Vector3 Vec3(float X, float Y, float Z) {
	return (Vector3) {
		X, Y, Z
	};
}

inline Vector2 Vec2(float X, float Y) {
	return (Vector2) {
		X, Y
	};
}

inline float Float_Vary(float A, float B, float C, Vector3 Bary) {
	return (A * Bary.X) + (B * Bary.Y) + (C * Bary.Z);
}

inline Vector3 Vector3_Sub(Vector3 A, Vector3 B) {
	return Vec3(A.X - B.X, A.Y - B.Y, A.Z - B.Z);
}

inline Vector3 Vector3_Normalize(Vector3 V) {
	float LenSq = V.X * V.X + V.Y * V.Y + V.Z * V.Z;
	float Len = sqrtf(LenSq);
	return Vec3(V.X / Len, V.Y / Len, V.Z / Len);
}

inline Vector3 Vector3_Cross(Vector3 A, Vector3 B) {
	return Vec3(A.Y * B.Z - A.Z * B.Y, A.Z * B.X - A.X * B.Z, A.X * B.Y - A.Y * B.X);
}

inline Vector3 Vector3_Transform(Vector3 V, Matrix4x4 Mat) {
	return Vec3(V.X * Mat.M11 + V.Y * Mat.M21 + V.Z * Mat.M31 + Mat.M41, V.X * Mat.M12 + V.Y * Mat.M22 + V.Z * Mat.M32 + Mat.M42, V.X * Mat.M13 + V.Y * Mat.M23 + V.Z * Mat.M33 + Mat.M43);
}

inline float Min(float A, float B, float C) {
	return min(A, min(B, C));
}

inline float Max(float A, float B, float C) {
	return max(A, max(B, C));
}

inline float Clamp(float V, float Min, float Max) {
	if (V < Min) return Min;
	if (V > Max) return Max;
	return V;
}

void BoundingBox(Vector3 A, Vector3 B, Vector3 C, Vector3* Minimum, Vector3* Maximum) {
	*Minimum = Vec3(Min(A.X, B.X, C.X), Min(A.Y, B.Y, C.Y), Min(A.Z, B.Z, C.Z));
	*Maximum = Vec3(Max(A.X, B.X, C.X), Max(A.Y, B.Y, C.Y), Max(A.Z, B.Z, C.Z));
}

inline Vector3 Barycentric(Vector3 A, Vector3 B, Vector3 C, int PX, int PY) {
	Vector3 U = Vector3_Cross(Vec3(C.X - A.X, B.X - A.X, A.X - PX), Vec3(C.Y - A.Y, B.Y - A.Y, A.Y - PY));
	Vector3 Val = { 0 };

	if (fabs(U.Z) < 1) {
		Val.X = -1;
		return Val;
	}

	Val.X = 1.0f - (U.X + U.Y) / U.Z;
	Val.Y = U.Y / U.Z;
	Val.Z = U.X / U.Z;
	return Val;
}

//-----------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------

RendrBuffer ColorBuffer;
RendrBuffer DepthBuffer;
RendrBuffer Tex0;

RendrColor DrawColor;

Matrix4x4 ViewMatrix;
Matrix4x4 ProjectionMatrix;
Matrix4x4 ModelMatrix;

static bool WireframeEnabled = false;
static bool EnableDepthTesting = true;
static bool EnableTexturing = true;
static bool EnableBackfaceCulling = true;

EXPORT RndrVertexBuffer* CreateVertexBuffer(Vector3* Points, Vector2* UVs, int Len) {
	RndrVertexBuffer* Ret = malloc(sizeof(RndrVertexBuffer));

	if (Ret == NULL || Points == NULL || UVs == NULL || Len <= 0)
		return NULL;

	Ret->Verts = malloc(sizeof(RndrVertex) * Len);

	if (Ret->Verts == NULL) {
		free(Ret);
		return NULL;
	}

	Ret->Length = Len;

	for (int i = 0; i < Len; i++)
	{
		Ret->Verts[i].Pos = Points[i];
		Ret->Verts[i].UV = UVs[i];
	}

	return Ret;
}

EXPORT void EnableWireframe(int Enable) {
	if (Enable != 0) {
		WireframeEnabled = true;
	} else {
		WireframeEnabled = false;
	}
}

EXPORT DeleteVertexBuffer(RndrVertexBuffer* Buffer) {
	free(Buffer->Verts);
	Buffer->Verts = NULL;
	Buffer->Length = 0;

	free(Buffer);
}

EXPORT void SetColorBuffer(RendrColor* Buffer, int Width, int Height) {
	ColorBuffer.Buffer = Buffer;
	ColorBuffer.Width = Width;
	ColorBuffer.Height = Height;
}

EXPORT void SetDepthBuffer(RendrColor* Buffer, int Width, int Height) {
	DepthBuffer.Buffer = Buffer;
	DepthBuffer.Width = Width;
	DepthBuffer.Height = Height;
}

inline RendrColor IndexBuffer(RendrBuffer* Buffer, float U, float V) {
	int Height = Buffer->Height;
	int Width = Buffer->Width;

	int Index = (int)(V * Height) * Width + (int)(U * Width);
	return Buffer->Buffer[Index];
}

EXPORT void SetTexBuffer(RendrColor* Buffer, int Width, int Height) {
	Tex0.Buffer = Buffer;
	Tex0.Width = Width;
	Tex0.Height = Height;
}

EXPORT void SetDrawColor(byte R, byte G, byte B, byte A) {
	DrawColor.R = R;
	DrawColor.G = G;
	DrawColor.B = B;
	DrawColor.A = A;
}

EXPORT void SetMatrix(Matrix4x4 Mat, int MatType) {
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

EXPORT void Clear(byte R, byte G, byte B, byte A, float Depth) {
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

EXPORT void DrawLine(int X0, int Y0, int X1, int Y1) {
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

void Shader_Vertex(Vector3* V) {
	Matrix4x4 FinalMatrix = Matrix4x4_Multiply(Matrix4x4_Multiply(ModelMatrix, ViewMatrix), ProjectionMatrix);
	*V = Vector3_Transform(*V, FinalMatrix);
}

RendrColor Shader_Fragment(Vector3 Pos, Vector2 UV) {
	if (EnableTexturing)
		return IndexBuffer(&Tex0, UV.X, UV.Y);

	return DrawColor;
}

inline void DrawTriangle(RndrVertex* TriangleVerts, int Index) {
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
		DrawLine((int)A.Pos.X, (int)A.Pos.Y, (int)B.Pos.X, (int)B.Pos.Y);
		DrawLine((int)B.Pos.X, (int)B.Pos.Y, (int)C.Pos.X, (int)C.Pos.Y);
		DrawLine((int)C.Pos.X, (int)C.Pos.Y, (int)A.Pos.X, (int)A.Pos.Y);
	} else {
		Vector3 Min;
		Vector3 Max;
		BoundingBox(A.Pos, B.Pos, C.Pos, &Min, &Max);

		for (int Y = (int)Min.Y; Y < Max.Y; Y++) {
			for (int X = (int)Min.X; X < Max.X; X++) {
				if (X < 0 || Y < 0 || X >= ColorBuffer.Width || Y >= ColorBuffer.Height)
					continue;


				// NOTE, replace the line below with this wrong function call, impacts performance A LOT
				// Vector3 BCnt = Barycentric(A.Pos, B.Pos, C.Pos, X, Y, &BCnt);
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

EXPORT void DrawTriangles(RndrVertexBuffer* Vertices) {
	int Count = Vertices->Length / 3;

	for (int i = 0; i < Count; i++)
		DrawTriangle(Vertices->Verts, i * 3);
}
