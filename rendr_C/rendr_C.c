#include <stdio.h>

#define EXPORT __declspec(dllexport)

EXPORT void Init() {
    printf("rendr from C\n");
}