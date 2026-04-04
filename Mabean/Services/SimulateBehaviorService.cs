using Mabean.Interop;
using System.Threading.Tasks;

namespace Mabean.Services
{
    public class SimulateBehaviorService
    {
        private readonly SimulationStepService _stepService;
        private readonly ReverseShellService _reverseShellService;

        public SimulateBehaviorService(SimulationStepService stepService, ReverseShellService reverseShellService)
        {
            _stepService = stepService;
            _reverseShellService = reverseShellService;
        }

        public async Task InjectBehavior(uint puid, string specificBehavior, string payloadName, string? programName)
        {
            LoggerService.Write($"[+] Simulating behavior: {specificBehavior} into process with PUID: {puid} using payload: {payloadName}");

            _stepService.BeginSimulation(specificBehavior);
            var callback = _stepService.GetCallback();

            switch (specificBehavior)
            {
                case "Injection-Simple":
                    var payload1 = await PayloadService.GetPayload(payloadName);
                    var code = await Task.Run(() => InteropInjection.InjectPayloadSimple(puid, payload1, (uint)payload1.Length, callback));
                    LoggerService.Write($"[+] Injected payload into process with PUID: {puid} with return code: {code}");
                    if (payloadName.StartsWith("reverse-shell-"))
                        _reverseShellService.StopPolling();
                    break;

                case "Injection-Apc-MultiThreaded":
                    var payload2 = await PayloadService.GetPayload(payloadName);
                    var code2 = await Task.Run(() => InteropInjection.InjectPayloadApcMultiThreaded(puid, payload2, (nuint)payload2.Length, callback));
                    LoggerService.Write($"[+] Injected payload into process with PUID: {puid} with return code: {code2}");
                    if (payloadName.StartsWith("reverse-shell-"))
                        _reverseShellService.StopPolling();
                    break;

                case "Injection-Apc-EarlyBird":
                    var payload3 = await PayloadService.GetPayload(payloadName);
                    var code3 = await Task.Run(() => InteropInjection.InjectPayloadApcEarlyBird(programName!, payload3, (nuint)payload3.Length, callback));
                    LoggerService.Write($"[+] Injected payload into process with PUID: {puid} with return code: {code3}");
                    if (payloadName.StartsWith("reverse-shell-"))
                        _reverseShellService.StopPolling();
                    break;

                case "PrivilegeEscalation-TokenTheft":
                    var code4 = await Task.Run(() => InteropPrivilegeEscalation.TokenTheftEscalation(puid, callback));
                    LoggerService.Write($"[+] Performed token theft escalation into process with PUID: {puid} with return code: {code4}");
                    break;

                case "PrivilegeEscalation-FodHelperAbuse":
                    var code5 = await Task.Run(() => InteropPrivilegeEscalation.FodHelperAbuseEscalation(null, callback));
                    LoggerService.Write($"[+] Performed FodHelper abuse escalation with return code: {code5}");
                    break;
            }
        }
    }
}
