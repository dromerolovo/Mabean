#pragma once
#include <windows.h>
#include <tlhelp32.h>
#include <stdio.h>
#include <wchar.h>
#include <shellapi.h>

typedef void (*StepCallback)(const char* stepName, int stepIndex);

int FodHelperAbuseEscalationInternal(const char* execPath, StepCallback callback);
