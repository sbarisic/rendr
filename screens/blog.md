# Speed comparison of C and C# binaries - Part 1

### Intro

The theme of this post is the speed comparison between two binaries. Both targeting x64 Windows operating system.
One is written in C# (.NET Framework 4.8), the other is written in C (Legacy MSVC Standard) compiled with Microsoft's Visual C compiler. All benchmarks done on Intel Core i7-8550U CPU.

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

The same can be done with any other static function, the .NET intermediate language already supports instructions for adding functions with their appropriate stubs to the dll export table. The catch is that the C# language itself does not support attributes like that by default, only ``DllImport``.

The tool used to add ``DllExport`` functionality in this demo is the following: https://github.com/3F/DllExport

### Demo

The demo application is a simple software renderer. It loads a model of a cup in ``obj`` format, a texture from a ``png`` file, some math magic happens and you get a stream of pixels out. The resulting image is rendered in a window using GLFW. GLFW also helps with keyboard input, window creation, event handling and so on.

![](https://github.com/sbarisic/rendr/raw/main/screens/demo_gif.gif)

The launcher uses either the C binary or the C# binary depending on the prompted input at the start of the program.
(Source: https://github.com/sbarisic/rendr/blob/main/Launcher/rendr.cs)

It binds the exported functions from the dll files to a set of delegates (basically; function pointers). After that, the main launcher program does all the model loading and proper API usage to get identical output from both of them on the screen.

(Source: https://github.com/sbarisic/rendr/blob/main/Launcher/Launcher.cs#L121-L191)

There is almost no difference between the C and C# version, basically copypasted code with minor differences just to make it compile. This is another reason why you can see actual old-school function pointers in .NET code :)

(C version: https://github.com/sbarisic/rendr/blob/main/rendr_C/rendr_C.c#L362-L417)
(C# version: https://github.com/sbarisic/rendr/blob/main/rendr_NET/rendr_NET.cs#L343-L395)

### Benchmarks

The whole solution was compiled in debug mode with all optimizations turned off, and in release mode with all optimizations turned on. All benchmark numbers are taken at the 1000th rendered frame to make it consistent. Stats were printed to the console once every 2 seconds.

### Debug mode, wireframe

The performance between these two was basically identical. Both rendering backends delivered the 1000th frame after about 26 seconds.

![](https://raw.githubusercontent.com/sbarisic/rendr/main/screens/static_screenshot_wireframe.png)

![](https://raw.githubusercontent.com/sbarisic/rendr/main/screens/c_debug_wireframe.png)

![](https://raw.githubusercontent.com/sbarisic/rendr/main/screens/csharp_debug_wireframe.png)

### Release mode, wireframe

Very similar performance, the native C version is about 4% faster here.
It's still not worth sacrificing all the new language features for this small performance gain.

![](https://raw.githubusercontent.com/sbarisic/rendr/main/screens/c_release_wireframe.png)

![](https://raw.githubusercontent.com/sbarisic/rendr/main/screens/csharp_release_wireframe.png)

### Debug mode, full rendering

Interesting! With no optimizations by the compiler, the .NET version is actually faster.
C binary rendered the 1000th frame after 692 seconds with 1.45 FPS average, while C# did the same in about 422 seconds with 2.38 FPS average.

![](https://raw.githubusercontent.com/sbarisic/rendr/main/screens/c_debug_fullshading.png)

![](https://raw.githubusercontent.com/sbarisic/rendr/main/screens/csharp_debug_fullshading.png)

### Release mode, full rendering

Now actual results everybody is interested in.
1000th frame in C delivered after 41 seconds. C# took twice as long.

![](https://raw.githubusercontent.com/sbarisic/rendr/main/screens/c_release_fullshading.png)

![](https://raw.githubusercontent.com/sbarisic/rendr/main/screens/csharp_release_fullshading.png)


### Conclusion

