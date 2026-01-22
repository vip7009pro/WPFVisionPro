using System.Net.Sockets;
using Modbus.Device;
using VisionPro.Core.Interfaces;

namespace VisionPro.Communication.Modbus;

public class ModbusTcpClient : ICommunicationClient
{
    private TcpClient? _tcpClient;
    private ModbusIpMaster? _master;
    private readonly string _ipAddress;
    private readonly int _port;
    
    public string ClientId { get; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "Modbus TCP Client";
    public bool IsConnected => _tcpClient?.Connected ?? false;
    
    public event EventHandler<byte[]>? DataReceived; // Not typically used for Modbus polling
    public event EventHandler<bool>? ConnectionStatusChanged;
    public event EventHandler<Exception>? ErrorOccurred;

    public ModbusTcpClient(string ipAddress, int port = 502)
    {
        _ipAddress = ipAddress;
        _port = port;
    }

    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (IsConnected) return true;
            
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(_ipAddress, _port);
            
            _master = ModbusIpMaster.CreateIp(_tcpClient);
            
            ConnectionStatusChanged?.Invoke(this, true);
            return true;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
            return false;
        }
    }

    public Task DisconnectAsync()
    {
        try
        {
            _tcpClient?.Close();
            _tcpClient?.Dispose();
            _master?.Dispose(); // NModbus master might need disposal if supported
            _tcpClient = null;
            _master = null;
            
            ConnectionStatusChanged?.Invoke(this, false);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
        }
        return Task.CompletedTask;
    }

    // Generic send/receive not applicable for standard Modbus usage via interface
    public Task<bool> SendAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        // Could interpret data as protocol command, but better to use specific methods
        return Task.FromResult(false);
    }

    public Task<byte[]?> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<byte[]?>(null);
    }

    // Modbus Specific Methods
    
    public async Task<bool[]> ReadCoilsAsync(byte slaveId, ushort startAddress, ushort numberOfPoints)
    {
        if (_master == null) throw new InvalidOperationException("Not connected");
        return await _master.ReadCoilsAsync(slaveId, startAddress, numberOfPoints);
    }
    
    public async Task<ushort[]> ReadHoldingRegistersAsync(byte slaveId, ushort startAddress, ushort numberOfPoints)
    {
        if (_master == null) throw new InvalidOperationException("Not connected");
        return await _master.ReadHoldingRegistersAsync(slaveId, startAddress, numberOfPoints);
    }
    
    public async Task WriteSingleCoilAsync(byte slaveId, ushort coilAddress, bool value)
    {
        if (_master == null) throw new InvalidOperationException("Not connected");
        await _master.WriteSingleCoilAsync(slaveId, coilAddress, value);
    }
    
    public async Task WriteSingleRegisterAsync(byte slaveId, ushort registerAddress, ushort value)
    {
        if (_master == null) throw new InvalidOperationException("Not connected");
        await _master.WriteSingleRegisterAsync(slaveId, registerAddress, value);
    }
    
    public void Dispose()
    {
        DisconnectAsync().Wait();
    }
}
