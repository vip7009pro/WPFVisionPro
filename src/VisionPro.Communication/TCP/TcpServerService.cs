using System.Net;
using System.Net.Sockets;
using System.Text;
using VisionPro.Core.Interfaces;

namespace VisionPro.Communication.TCP;

public class TcpServerService : IDisposable
{
    private TcpListener? _listener;
    private CancellationTokenSource? _cts;
    private Task? _listenerTask;
    private readonly List<TcpClient> _clients = new();
    
    public event EventHandler<string>? DataReceived;
    public event EventHandler<string>? ErrorOccurred;
    public event EventHandler<string>? ClientConnected;
    public event EventHandler<string>? ClientDisconnected;
    
    public bool IsListening { get; private set; }
    public int Port { get; private set; }
    
    public void Start(int port)
    {
        if (IsListening) return;
        
        try
        {
            Port = port;
            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
            IsListening = true;
            
            _cts = new CancellationTokenSource();
            _listenerTask = Task.Run(() => AcceptClientsAsync(_cts.Token));
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Failed to start server: {ex.Message}");
            IsListening = false;
        }
    }
    
    public void Stop()
    {
        if (!IsListening) return;
        
        _cts?.Cancel();
        _listener?.Stop();
        
        lock (_clients)
        {
            foreach (var client in _clients)
            {
                client.Close();
            }
            _clients.Clear();
        }
        
        IsListening = false;
    }
    
    private async Task AcceptClientsAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (_listener == null) break;
                
                var client = await _listener.AcceptTcpClientAsync(token);
                ClientConnected?.Invoke(this, client.Client.RemoteEndPoint?.ToString() ?? "Unknown");
                
                lock (_clients)
                {
                    _clients.Add(client);
                }
                
                _ = HandleClientAsync(client, token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Accept error: {ex.Message}");
            }
        }
    }
    
    private async Task HandleClientAsync(TcpClient client, CancellationToken token)
    {
        var buffer = new byte[1024];
        var stream = client.GetStream();
        var endPoint = client.Client.RemoteEndPoint?.ToString();
        
        try
        {
            while (client.Connected && !token.IsCancellationRequested)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                if (bytesRead == 0) break;
                
                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                DataReceived?.Invoke(this, message);
                
                // Echo for testing
                // await stream.WriteAsync(buffer, 0, bytesRead, token);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorOccurred?.Invoke(this, $"Client error: {ex.Message}");
        }
        finally
        {
            ClientDisconnected?.Invoke(this, endPoint ?? "Unknown");
            lock (_clients)
            {
                _clients.Remove(client);
            }
            client.Close();
        }
    }

    public void Dispose()
    {
        Stop();
        _cts?.Dispose();
    }
}
