# Prepare the C build tools
## Download the C Compiler
- Download [gcc 14.2.0](https://github.com/brechtsanders/winlibs_mingw/releases/download/14.2.0posix-19.1.1-12.0.0-ucrt-r2/winlibs-x86_64-posix-seh-gcc-14.2.0-llvm-19.1.1-mingw-w64ucrt-12.0.0-r2.7z) from [winlibs](https://winlibs.com/#download-release)
    - Extract files to C:\Program Files\mingw64

    - Add "C:\Program Files\mingw64\bin" to your PATH environment variables

## Download CMAKE
- Download [CMAKE 3.31.1](https://cmake.org/download/) from their website.

    - For 64 bit Windows this should be the file "Windows x64 Installer"

    - Ensure the "Add CMake to the PATH environment varible" option is selected

## Building the C project manually
Start in the same folder as the "src" folder and run:
- mkdir build
- cd build
- cmake -G "MinGW Makefiles" ../src
- mingw32-make install