#include <stdio.h>
#include <stdlib.h>
#include <stdbool.h>

#define EXPORT __declspec(dllexport)

typedef unsigned char byte;

typedef struct {
	byte B;
	byte G;
	byte R;
	byte A;
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

//------------ Generic Utility Functions --------------------------------------------------

Vector3 Vector3_Add(Vector3 A, Vector3 B) {
	return (Vector3) { A.X + B.X, A.Y + B.Y, A.Z + B.Z };
}

Vector3 Vector3_Normalize(Vector3 V) {
	return (Vector3) {0, 0, 0};
}

Vector3 Vector3_Cross() {

}

//-----------------------------------------------------------------------------------------

RendrBuffer ColorBuffer;
RendrColor DrawColor;

bool EnableWireframe = false;
bool EnableDepthTesting = false;
bool EnableTexturing = false;
bool EnableBackfaceCulling = false;

EXPORT void Init() {
	printf("rendr from C\n");

}

EXPORT void SetColorBuffer(RendrColor* Buffer, int Width, int Height) {
	ColorBuffer.Buffer = Buffer;
	ColorBuffer.Width = Width;
	ColorBuffer.Height = Height;
}

EXPORT void SetDrawColor(byte R, byte G, byte B, byte A) {
	DrawColor.R = R;
	DrawColor.G = G;
	DrawColor.B = B;
	DrawColor.A = A;
}

EXPORT void Fill(byte R, byte G, byte B, byte A) {
	int Len = ColorBuffer.Width * ColorBuffer.Height;

	for (int i = 0; i < Len; i++)
	{
		ColorBuffer.Buffer[i].R = R;
		ColorBuffer.Buffer[i].G = G;
		ColorBuffer.Buffer[i].B = B;
		ColorBuffer.Buffer[i].A = A;
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
		if (Steep)
			ColorBuffer.Buffer[X * ColorBuffer.Width + Y] = DrawColor;
		else
			ColorBuffer.Buffer[Y * ColorBuffer.Width + X] = DrawColor;


		Error2 += DeltaError2;

		if (Error2 > DeltaX) {
			Y += (Y1 > Y0 ? 1 : -1);
			Error2 -= DeltaX * 2;
		}
	}
}

EXPORT void DrawTriangle(Vector3* Vertices, Vector2* UVs, int Index) {
	Vector3 A = Vertices[Index];
	Vector3 B = Vertices[Index + 1];
	Vector3 C = Vertices[Index + 2];

	const float Mul = 40;

	DrawLine((int)A.X * Mul, (int)A.Y * Mul, (int)B.X * Mul, (int)B.Y * Mul);

	DrawLine((int)B.X * Mul, (int)B.Y * Mul, (int)C.X * Mul, (int)C.Y * Mul);

	DrawLine((int)C.X * Mul, (int)C.Y * Mul, (int)A.X * Mul, (int)A.Y * Mul);
}