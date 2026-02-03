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

        public async Task InjectBehavior(uint puid, string specificBehavior, string payloadName)
        {
            LoggerService.Write($"[+] Simulating behavior: {specificBehavior} into process with PUID: {puid} using payload: {payloadName}");
            switch (specificBehavior) 
            {
                case "Injection-Simple":
                    var code = InteropInjection.InjectPayload(puid, await PayloadService.GetPayload(payloadName), (uint)(await PayloadService.GetPayload(payloadName)).Length);
                    LoggerService.Write($"[+] Injected payload into process with PUID: {puid} with return code: {code}");
                    break;
            }

        }
    }
}
