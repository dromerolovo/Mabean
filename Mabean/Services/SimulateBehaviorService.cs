using Mabean.Interop;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mabean.Services
{
    public class SimulateBehaviorService
    {
        private readonly PayloadService _payloadService;

        public SimulateBehaviorService(PayloadService payloadService)
        {
            _payloadService = payloadService;
        }

        public async Task InjectBehavior(uint puid, string specificBehavior, string payloadName, string? programName)
        {
            LoggerService.Write($"[+] Simulating behavior: {specificBehavior} into process with PUID: {puid} using payload: {payloadName}");
            switch (specificBehavior) 
            {
                case "Injection-Simple":
                    var code = InteropInjection.InjectPayloadSimple(puid, await PayloadService.GetPayload(payloadName), (uint)(await PayloadService.GetPayload(payloadName)).Length);
                    LoggerService.Write($"[+] Injected payload into process with PUID: {puid} with return code: {code}");
                    break;
                case "Injection-Apc-MultiThreaded":
                    var code2 = InteropInjection.InjectPayloadApcMultiThreaded(puid, await PayloadService.GetPayload(payloadName), (nuint)(await PayloadService.GetPayload(payloadName)).Length);
                    LoggerService.Write($"[+] Injected payload into process with PUID: {puid} with return code: {code2}");
                    break;
                case "Injection-Apc-EarlyBird":
                    var code3 = InteropInjection.InjectPayloadApcEarlyBird(programName!, await PayloadService.GetPayload(payloadName), (nuint)(await PayloadService.GetPayload(payloadName)).Length);
                    Console.WriteLine(code3);
                    LoggerService.Write($"[+] Injected payload into process with PUID: {puid} with return code: {code3}");
                    break;
                case "PrivilegeEscalation-TokenTheft":
                    var code4 = InteropPrivilegeEscalation.TokenTheftEscalation(puid);
                    LoggerService.Write($"[+] Performed token theft escalation into process with PUID: {puid} with return code: {code4}");
                    break;
            }
        }
    }
}
