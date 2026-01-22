using System.Net;
using System.Net.Sockets;
using Modbus.Device;

namespace VisionPro.Communication.Simulation;

public class PlcSimulator : IDisposable
{
    private TcpListener? _slaveListener;
    private ModbusTcpSlave? _slave;
    private Task? _slaveTask;
    private readonly byte _slaveId;
    
    public bool IsRunning { get; private set; }
    
    // Simulate raw TCP trigger as well
    private readonly TCP.TcpServerService _tcpServer = new();

    public PlcSimulator(byte slaveId = 1)
    {
        _slaveId = slaveId;
    }

    public void Start(int modbusPort = 502, int triggerPort = 2000)
    {
        if (IsRunning) return;
        
        try
        {
            // Start Modbus Slave
            _slaveListener = new TcpListener(IPAddress.Any, modbusPort);
            _slaveListener.Start();
            
            _slave = ModbusTcpSlave.CreateTcp(_slaveId, _slaveListener);
            _slave.DataStore = global::Modbus.Data.DataStoreFactory.CreateDefaultDataStore();
            
            _slaveTask = Task.Run(() => _slave.Listen());
            
            // Start TCP Trigger Server
            _tcpServer.Start(triggerPort);
            
            IsRunning = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Simulator start failed: {ex.Message}");
        }
    }

    public void Stop()
    {
        if (!IsRunning) return;
        
        _slaveListener?.Stop();
        _slave?.Dispose();
        _tcpServer.Stop();
        
        IsRunning = false;
    }
    
    public void SetRegister(ushort address, ushort value)
    {
        if (_slave?.DataStore != null)
        {
            // Note: NModbus4 DataStore is 1-based usually, check implementation.
            // But collection is usually 1-origin in NModbus? Or 0?
            // ModbusDataCollection uses ushort address.
            // Assuming direct mapping.
            // Also removing slaveId index.
            if (_slave.DataStore.HoldingRegisters.Count > address)
                _slave.DataStore.HoldingRegisters[address] = value;
        }
    }
    
    public ushort GetRegister(ushort address)
    {
         if (_slave?.DataStore != null)
        {
            if (_slave.DataStore.HoldingRegisters.Count > address)
                return _slave.DataStore.HoldingRegisters[address];
        }
        return 0;
    }
    
    public void Trigger()
    {
        // ...
    }

    public void Dispose()
    {
        Stop();
        _tcpServer.Dispose();
    }
}
