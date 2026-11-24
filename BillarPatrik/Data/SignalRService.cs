using Microsoft.AspNetCore.SignalR.Client;

namespace BillarPatrik.Data
{
    public class SignalRService
    {
        private readonly SemaphoreSlim _connectionLock = new(1, 1);
        private HubConnection? _hubConnection;
        public event Action<int, string, string> MesaActualizada;
        public event Action<int, decimal> NuevaVentaRecibida;
        public event Action<HubConnectionState>? EstadoConexionCambiado;
        public event Action<string>? ErrorConexion;

        public HubConnectionState Estado =>
            _hubConnection?.State ?? HubConnectionState.Disconnected;

        public async Task IniciarConexion()
        {
            await _connectionLock.WaitAsync();
            try
            {
                if (_hubConnection == null)
                {
                    _hubConnection = new HubConnectionBuilder()
                        .WithUrl(ApiSettings.HubUri)
                        .WithAutomaticReconnect(new[]
                        {
                            TimeSpan.FromSeconds(2),
                            TimeSpan.FromSeconds(5),
                            TimeSpan.FromSeconds(10),
                            TimeSpan.FromSeconds(20)
                        })
                        .Build();

                    _hubConnection.On<int, string, string>("RecibirActualizacionMesa", (numeroMesa, estado, cliente) =>
                        MesaActualizada?.Invoke(numeroMesa, estado, cliente));
                    _hubConnection.On<int, decimal>("RecibirNuevaVenta", (numeroMesa, total) =>
                        NuevaVentaRecibida?.Invoke(numeroMesa, total));

                    _hubConnection.Closed += ex =>
                    {
                        EstadoConexionCambiado?.Invoke(HubConnectionState.Disconnected);
                        ErrorConexion?.Invoke(ex?.Message ?? "Se perdió la conexión con el servidor.");
                        return Task.CompletedTask;
                    };
                    _hubConnection.Reconnecting += ex =>
                    {
                        EstadoConexionCambiado?.Invoke(HubConnectionState.Reconnecting);
                        ErrorConexion?.Invoke(ex?.Message ?? "Reconectando con el servidor...");
                        return Task.CompletedTask;
                    };
                    _hubConnection.Reconnected += _ =>
                    {
                        EstadoConexionCambiado?.Invoke(HubConnectionState.Connected);
                        return Task.CompletedTask;
                    };
                }

                if (_hubConnection.State is HubConnectionState.Connected
                    or HubConnectionState.Connecting
                    or HubConnectionState.Reconnecting)
                    return;

                EstadoConexionCambiado?.Invoke(HubConnectionState.Connecting);
                await _hubConnection.StartAsync();
                EstadoConexionCambiado?.Invoke(HubConnectionState.Connected);
            }
            catch (Exception ex)
            {
                EstadoConexionCambiado?.Invoke(HubConnectionState.Disconnected);
                ErrorConexion?.Invoke(ex.Message);
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public async Task EnviarActualizacionMesa(int numeroMesa, string estado, string cliente)
        {
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.SendAsync("EnviarActualizacionMesa", numeroMesa, estado, cliente);
            }
        }

        public async Task EnviarNuevaVenta(int numeroMesa, decimal total)
        {
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.SendAsync("EnviarNuevaVenta", numeroMesa, total);
            }
        }

        public async Task DetenerConexion()
        {
            await _connectionLock.WaitAsync();
            try
            {
                if (_hubConnection?.State is HubConnectionState.Connected
                    or HubConnectionState.Connecting
                    or HubConnectionState.Reconnecting)
                    await _hubConnection.StopAsync();
                EstadoConexionCambiado?.Invoke(HubConnectionState.Disconnected);
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public async Task<bool> VerificarApi()
        {
            try
            {
                using var client = new HttpClient
                {
                    BaseAddress = ApiSettings.ApiUri,
                    Timeout = TimeSpan.FromSeconds(5)
                };
                using var response = await client.GetAsync("/WeatherForecast");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}