namespace VisionPro.Core.Interfaces;

/// <summary>
/// Interface for communication clients (TCP, Modbus, OPC UA)
/// </summary>
public interface ICommunicationClient : IDisposable
{
    /// <summary>
    /// Unique identifier for this client
    /// </summary>
    string ClientId { get; }
    
    /// <summary>
    /// Display name of the client
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Indicates whether the client is connected
    /// </summary>
    bool IsConnected { get; }
    
    /// <summary>
    /// Connect to the remote endpoint
    /// </summary>
    Task<bool> ConnectAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Disconnect from the remote endpoint
    /// </summary>
    Task DisconnectAsync();
    
    /// <summary>
    /// Send data to the remote endpoint
    /// </summary>
    /// <param name="data">Data to send</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> SendAsync(byte[] data, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Receive data from the remote endpoint
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<byte[]?> ReceiveAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Event raised when data is received
    /// </summary>
    event EventHandler<byte[]>? DataReceived;
    
    /// <summary>
    /// Event raised when connection status changes
    /// </summary>
    event EventHandler<bool>? ConnectionStatusChanged;
    
    /// <summary>
    /// Event raised when an error occurs
    /// </summary>
    event EventHandler<Exception>? ErrorOccurred;
}
