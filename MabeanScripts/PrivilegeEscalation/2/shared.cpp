#include "pch.h"
#include "shared.h"

#include <shellapi.h>
#include <windows.h>
#include <tlhelp32.h>
#include <stdio.h>
#include <wchar.h>

#pragma comment(lib, "Advapi32.lib")

DWORD FindExplorerPidInMySession()
{
    DWORD mySession = 0;
    if (!ProcessIdToSessionId(GetCurrentProcessId(), &mySession))
        return 0;

    HANDLE snap = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (snap == INVALID_HANDLE_VALUE) return 0;

    PROCESSENTRY32W pe{};
    pe.dwSize = sizeof(pe);

    DWORD pid = 0;
    for (BOOL ok = Process32FirstW(snap, &pe); ok; ok = Process32NextW(snap, &pe))
    {
        if (_wcsicmp(pe.szExeFile, L"explorer.exe") == 0)
        {
            DWORD s = 0;
            if (ProcessIdToSessionId(pe.th32ProcessID, &s) && s == mySession)
            {
                pid = pe.th32ProcessID;
                break;
            }
        }
    }

    CloseHandle(snap);
    return pid;
}
void Marker() {
    STARTUPINFO si = { 0 };
    PROCESS_INFORMATION pi = { 0 };

    si.cb = sizeof(si);

    wchar_t appPath[MAX_PATH] = { 0 };

    if (!ExpandEnvironmentStringsW(L"%LOCALAPPDATA%\\Mabean\\data\\MabeanMarker.exe", appPath, MAX_PATH)) {
        printf("ExpandEnvironmentStrings failed: %lu\n", GetLastError());
    }

    if (!CreateProcessW(
        appPath,
        NULL,
        NULL,
        NULL,
        FALSE,
        2,
        NULL,
        NULL,
        &si,
        &pi))
    {
        printf("CreateProcess failed: %lu\n", GetLastError());
    }

    printf("Process started with PID: %lu\n", pi.dwProcessId);

    CloseHandle(pi.hProcess);
    CloseHandle(pi.hThread);
}