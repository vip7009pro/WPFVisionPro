using Opc.Ua;
using Opc.Ua.Client;
using VisionPro.Core.Interfaces;

namespace VisionPro.Communication.OpcUa;

public class OpcUaClient : ICommunicationClient
{
    private Session? _session;
    private readonly string _endpointUrl;
    private readonly ApplicationConfiguration _config;
    
    public string ClientId { get; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "OPC UA Client";
    public bool IsConnected => _session?.Connected ?? false;
    
    public event EventHandler<byte[]>? DataReceived;
    public event EventHandler<bool>? ConnectionStatusChanged;
    public event EventHandler<Exception>? ErrorOccurred;

    public OpcUaClient(string endpointUrl)
    {
        _endpointUrl = endpointUrl;
        
        _config = new ApplicationConfiguration
        {
            ApplicationName = "VisionProClient",
            ApplicationType = ApplicationType.Client,
            SecurityConfiguration = new SecurityConfiguration
            {
                ApplicationCertificate = new CertificateIdentifier(),
                AutoAcceptUntrustedCertificates = true // For dev/test
            },
            TransportConfigurations = new TransportConfigurationCollection(),
            TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
            ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 }
        };
        _config.Validate(ApplicationType.Client).Wait();
    }

    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (IsConnected) return true;
            
            // Simplified endpoint selection:
            var discoveryUrl = _endpointUrl;
            if (!discoveryUrl.StartsWith("opc.tcp://")) discoveryUrl = "opc.tcp://" + discoveryUrl;
            
            // Since we can't easily discover without a client, we will assume generic config
             var endpoint = new EndpointDescription(discoveryUrl);
             var endpointConfig = EndpointConfiguration.Create(_config);
             var configuredEndpoint = new ConfiguredEndpoint(null, endpoint, endpointConfig);

            _session = await Session.Create(_config, configuredEndpoint, 
                false, "VisionPro Session", 60000, null, null);
            
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
             _session?.Close();
             _session?.Dispose();
             _session = null;
             ConnectionStatusChanged?.Invoke(this, false);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
        }
        return Task.CompletedTask;
    }

    public Task<bool> SendAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public Task<byte[]?> ReceiveAsync(CancellationToken cancellationToken = default)
    {
         return Task.FromResult<byte[]?>(null);
    }

    // OPC UA Specifics
    public async Task<object?> ReadNodeAsync(string nodeId)
    {
        if (_session == null) throw new InvalidOperationException("Not connected");
        
        var readValueId = new ReadValueId 
        { 
            NodeId = new NodeId(nodeId), 
            AttributeId = Attributes.Value 
        };
        
        var request = new ReadRequest 
        { 
            NodesToRead = new ReadValueIdCollection { readValueId }
        };
        
        var response = await _session.ReadAsync(null, 0, TimestampsToReturn.Both, request.NodesToRead, CancellationToken.None);
        
        if (response.Results[0].StatusCode == StatusCodes.Good)
        {
            return response.Results[0].Value;
        }
        
        return null;
    }
    
    public async Task WriteNodeAsync(string nodeId, object value)
    {
         if (_session == null) throw new InvalidOperationException("Not connected");
         
         var writeValue = new WriteValue
         {
             NodeId = new NodeId(nodeId),
             AttributeId = Attributes.Value,
             Value = new DataValue(new Variant(value))
         };
         
         var request = new WriteRequest
         {
             NodesToWrite = new WriteValueCollection { writeValue }
         };
         
         await _session.WriteAsync(null, request.NodesToWrite, CancellationToken.None);
    }

    public void Dispose()
    {
        DisconnectAsync().Wait();
    }
}
