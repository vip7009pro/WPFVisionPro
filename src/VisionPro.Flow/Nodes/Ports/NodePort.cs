namespace VisionPro.Flow.Nodes.Ports;

public enum PortType
{
    Image,          // Mat/Bitmap
    Coordinates,    // CoordinateSystem (Position/Rotation)
    Data,           // Numbers, Strings, Custom Objects
    Boolean         // Trigger/Logic
}

public enum PortDirection
{
    Input,
    Output
}

public class NodePort
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public PortType Type { get; set; }
    public PortDirection Direction { get; set; }
    public string ParentNodeId { get; set; } = string.Empty;
    
    // For connection validation and visual styling
    public bool IsConnected { get; set; }
    
    public NodePort(string name, PortType type, PortDirection direction, string parentNodeId)
    {
        Name = name;
        Type = type;
        Direction = direction;
        ParentNodeId = parentNodeId;
    }
}
