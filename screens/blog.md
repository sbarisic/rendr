# Speed comparison of C and C# programming languages

The theme of this post is the speed comparison between two binaries. Both targeting x64 Windows operating system.
One is written in C# (.NET Framework 4.8), the other is written in C (Legacy MSVC Standard) compiled with Microsoft's Visual C compiler.

Both of these .dll binaries export the same API and can be used from any other external program.
You may be familiar with how something like that can be done in a native language like C. It's easy.
All you have to do is the following

```c
__declspec(dllexport) void test_func() {
    // DO SOMETHING
}
```