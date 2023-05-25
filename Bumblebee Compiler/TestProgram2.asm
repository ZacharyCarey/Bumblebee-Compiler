;-------------------------------------------------------------------------------------------------------------------
; Hello, Windows!  in x86 ASM - (c) 2021 Dave's Garage - Use at your own risk, no warranty!
;-------------------------------------------------------------------------------------------------------------------

; Compiler directives and includes

.386						; Full 80386 instruction set and mode
.model flat, stdcall				; All 32-bit and later apps are flat. Used to include "tiny, etc"
option casemap:none				; Preserve the case of system identifiers but not our own, more or less


; Include files - headers and libs that we need for calling the system dlls like user32, gdi32, kernel32, etc
include \masm32\include\windows.inc		; Main windows header file (akin to Windows.h in C)
include \masm32\include\user32.inc		; Windows, controls, etc
include \masm32\include\kernel32.inc		; Handles, modules, paths, etc
include \masm32\include\gdi32.inc		; Drawing into a device context (ie: painting)
include \masm32\include\msvcrt.inc

; Libs - information needed to link ou binary to the system DLL callss

includelib \masm32\lib\kernel32.lib		; Kernel32.dll
includelib \masm32\lib\user32.lib		; User32.dll
includelib \masm32\lib\gdi32.lib		; GDI32.dll
includelib \masm32\lib\msvcrt.lib

; Forward declarations - Our main entry point will call forward to WinMain, so we need to define it here

printf proto c, :VARARG	; Forward decl for MainEntry 
exit proto c, :DWORD

.DATA

fmt db "%s", 10, 0
hello db "Hello, world!", 0

;-------------------------------------------------------------------------------------------------------------------
.CODE						; Here is where the program itself lives
;-------------------------------------------------------------------------------------------------------------------

main proc
	call output
	invoke exit, 0
main endp

output proc
	invoke printf, OFFSET fmt, OFFSET hello, eax
	ret
output ENDP

END main