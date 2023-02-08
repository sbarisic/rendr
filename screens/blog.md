# Speed comparison of C and C# programming languages

The theme of this post is the speed comparison between two binaries. Both targeting x64 Windows operating system.
One is written in C# (.NET Framework 4.8), the other is written in C (Legacy MSVC Standard) compiled with Microsoft's Visual C compiler.

Both of these .dll binaries export the same API and can be used from any other external program.
You may be familiar with how something like that can be done in a native language like C. It's easy.
All you have to do is the following:

```c
__declspec(dllexport) void test_func() {
    // DO SOMETHING
}
```

And any other program which wants to use the exported function can do that by using the WinAPI functions LoadLibrary/GetProcAddress, like this

```c
void main() {
    void* module = (void*)LoadLibrary("test.dll");
    void (*test_func)() = (void (*)())GetProcAddress(module, "test_func");
    
    test_func();
}
```

But C#? Isn't C# backed by some sort of intermediate language running on some sort of virtual machine?
Well, yes it is! But that does not mean you can't export functions from dll files written in C# and use them in a pure-C program. No, you don't even have to set up the .NET runtime yourself to handle that.

.NET exe programs export a native stub function (main() basically) which bootstraps the .NET runtime in the background, and the .NET runtime actually handles all the custom "VM" stuff. It does Just-in-Time compilation, optimization and many more things. This can be very powerful, as things like vector, matrix operations can be just-in-time compiled to the platform-specific most optimized native code (Intel vs AMD, AVX512, AVX2, AVX, SSE4.1, SSE2, SSE and so on).

