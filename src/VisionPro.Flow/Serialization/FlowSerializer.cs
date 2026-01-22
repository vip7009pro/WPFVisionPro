using System.Text.Json;
using VisionPro.Core.Interfaces;
using VisionPro.Core.Models;
using VisionPro.Core.Enums;
using VisionPro.Flow.Nodes;
using VisionPro.Flow.Nodes.Base;

namespace VisionPro.Flow.Serialization;

public class FlowSerializer
{
    private readonly JsonSerializerOptions _jsonOptions;

    public FlowSerializer()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Save flow to a JSON file
    /// </summary>
    public async Task SaveAsync(FlowDefinition flow, string filePath)
    {
        using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, flow, _jsonOptions);
    }

    /// <summary>
    /// Load flow from a JSON file
    /// </summary>
    public async Task<FlowDefinition?> LoadAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<FlowDefinition>(stream, _jsonOptions);
    }

    /// <summary>
    /// Convert runtime IFlowNodes to serializable FlowDefinition
    /// </summary>
    public FlowDefinition ConvertToDefinition(List<IFlowNode> nodes, string name = "My Flow")
    {
        var definition = new FlowDefinition
        {
            Name = name,
            Nodes = new List<FlowNodeDefinition>(),
            Connections = new List<FlowConnection>()
        };

        foreach (var node in nodes)
        {
            var nodeDef = new FlowNodeDefinition
            {
                NodeId = node.NodeId,
                NodeType = node.NodeType,
                Name = node.Name,
                PositionX = node.PositionX,
                PositionY = node.PositionY,
                Inputs = node.InputNodeIds,
                Outputs = node.OutputNodeIds
            };
            
            // Get node configuration
            var config = node.GetConfiguration();
            if (config.ValueKind != JsonValueKind.Undefined)
            {
                 // Convert JsonElement back to dictionary for serialization
                 // Note: This is a simplification. Ideally GetConfiguration would return a serializable object
                 // or we use JsonElement directly if FlowNodeDefinition supported it.
                 // For now, let's just Serialize the JsonElement to a Dictionary if possible,
                 // or just keep it as an object if we change FlowNodeDefinition.
                 
                 // Strategy: Let's assume GetConfiguration returns a JSON structure that we can deserialize to Dictionary
                 try 
                 {
                     var json = config.GetRawText();
                     var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                     if (dict != null)
                     {
                         nodeDef.Config = dict;
                     }
                 }
                 catch
                 {
                     // Fallback or empty config
                 }
            }

            definition.Nodes.Add(nodeDef);

            // Create connections based on Outputs
            // Limitation: This assumes simple output-to-input linkage based on NodeIds.
            // A more complex graph might use specific ports.
            foreach (var targetId in node.OutputNodeIds)
            {
                definition.Connections.Add(new FlowConnection
                {
                    SourceNodeId = node.NodeId,
                    TargetNodeId = targetId
                });
            }
        }

        return definition;
    }
    
    /// <summary>
    /// Create runtime IFlowNode instances from FlowDefinition
    /// </summary>
    public List<IFlowNode> CreateNodes(FlowDefinition definition)
    {
        var nodes = new List<IFlowNode>();
        
        foreach (var nodeDef in definition.Nodes)
        {
            IFlowNode? node = nodeDef.NodeType switch
            {
                FlowNodeType.InputImage => new InputImageNode(),
                FlowNodeType.TeachMatch => new TeachMatchNode(),
                FlowNodeType.Measurement => new MeasurementNode(),
                FlowNodeType.DefectDetection => new DefectDetectionNode(),
                FlowNodeType.ROIApply => new ROIApplyNode(),
                FlowNodeType.FinalDecision => new FinalDecisionNode(),
                // Add factories for other types as needed
                _ => null
            };
            
            if (node != null)
            {
                // Restore base properties (hacky, ideally via interface setter or constructor)
                // Since IFlowNode properties are mostly get-only in some designs, we need to cast to Base or use setters
                // Let's assume FlowNodeBase or properties have setters as per interface check earlier.
                // Checked IFlowNode: Name, PositionX, PostionY have setters. 
                // NodeId is Get Only. We might need a way to set it, or it generates new one.
                // Ideally we should be able to restore the ID for connections to work.
                
                if (node is FlowNodeBase baseNode)
                {
                    // Reflection or internal/public setter needed if NodeId is private set
                    // But wait, FlowNodeBase usually has protected set.
                    // For now, let's assume we can set it or it uses the config.
                    
                    // Actually, let's look at FlowNodeBase. It likely generates a GUID in constructor.
                    // We need to override it to match the saved ID.
                    SetNodeId(baseNode, nodeDef.NodeId);
                }
                
                node.Name = nodeDef.Name;
                node.PositionX = nodeDef.PositionX;
                node.PositionY = nodeDef.PositionY;
                
                // Restore connections
                // Note: IFlowNode Inputs/Outputs are typically ReadOnly lists managed by Connect/Disconnect methods
                // So we will rebuild connections later based on flowDefinition.Connections
                
                // Configure
                if (nodeDef.Config != null && nodeDef.Config.Count > 0)
                {
                    var configJson = JsonSerializer.Serialize(nodeDef.Config);
                    using var doc = JsonDocument.Parse(configJson);
                    node.Configure(doc.RootElement.Clone());
                }
                
                nodes.Add(node);
            }
        }
        
        // Rebuild connections
        foreach (var connection in definition.Connections)
        {
            var source = nodes.FirstOrDefault(n => n.NodeId == connection.SourceNodeId);
            var target = nodes.FirstOrDefault(n => n.NodeId == connection.TargetNodeId);
            
            if (source != null && target != null)
            {
                // Assuming we have a way to connect them. 
                // IFlowNode doesn't have a Connect method in the interface (checked earlier).
                // But FlowEngine manages execution topology.
                // Actually the nodes usually store their connections.
                // Let's check FlowNodeBase to see how connections are managed.
                // If they are just lists, we might need to cast to list and add.
                
                if (source is FlowNodeBase sourceBase)
                {
                    sourceBase.OutputNodeIds.Add(target.NodeId);
                }
                if (target is FlowNodeBase targetBase)
                {
                    targetBase.InputNodeIds.Add(source.NodeId);
                }
            }
        }
        
        return nodes;
    }
    
    private void SetNodeId(FlowNodeBase node, string id)
    {
        // Use reflection to set private/protected NodeId if necessary
        var prop = typeof(FlowNodeBase).GetProperty("NodeId");
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(node, id);
        }
        else
        {
            // Backing field? Or just accept new ID and remap? 
            // Better to force it via reflection on backing field if property is read-only
            var field = typeof(FlowNodeBase).GetField("<NodeId>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(node, id);
            }
        }
    }
}
