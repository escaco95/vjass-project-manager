using System.Windows.Media.Imaging;
using vJassMainJBlueprint.Utils;
using vJassMainJBlueprint.V1.Config;
using vJassMainJBlueprint.V1.Model;

namespace vJassMainJBlueprint.V1.ModelFacade
{
    public partial class ProjectEditFacade
    {
        private readonly OriginConfig _originConfig = new();
        private readonly ProjectConfig _projectConfig = new();
        private readonly NodeCollectionConfig _nodeCollectionConfigs = new();

        public enum OriginType
        {
            None,
            Memory,
            File,
        }

        internal class OriginConfig
        {
            public OriginType Origin = OriginType.Memory;
            public string OriginFilePath = string.Empty;
            public bool OriginSaveRequired = false;

            public void Reset()
            {
                Origin = OriginType.Memory;
                OriginFilePath = string.Empty;
                OriginSaveRequired = false;
            }

            public void ResetToFile(string filePath)
            {
                Origin = OriginType.File;
                OriginFilePath = filePath;
                OriginSaveRequired = false;
            }
        }

        internal class ProjectConfig
        {
            public string Name = JassProjectSpecification.DefaultProjectName;
            public string Author = JassProjectSpecification.DefaultProjectAuthor;
            public int Width = JassProjectSpecification.DefaultProjectWidth;
            public int Height = JassProjectSpecification.DefaultProjectHeight;

            public void Reset()
            {
                Name = JassProjectSpecification.DefaultProjectName;
                Author = JassProjectSpecification.DefaultProjectAuthor;
                Width = JassProjectSpecification.DefaultProjectWidth;
                Height = JassProjectSpecification.DefaultProjectHeight;
            }

            public void Consume(JassProject jassProject)
            {
                Name = jassProject.Name;
                Author = jassProject.Author;
                Width = jassProject.Width;
                Height = jassProject.Height;
            }

            public JassProject Cast(JassProject.Node[] nodes)
            {
                return new JassProject(Name, Author, Width, Height, nodes);
            }
        }

        internal class NodeCollectionConfig
        {
            public long NodeHandleId = 0x100000;
            public Dictionary<long, NodeConfigEntity> NodeConfigs = [];
            public Dictionary<string, NodeConfigEntity> IndexNodeSourceFile = [];

            public void Reset()
            {
                NodeHandleId = 0x100000;
                NodeConfigs.Clear();
            }

            public void Consume(JassProject jassProject)
            {
                NodeHandleId = 0x100000;
                NodeConfigs.Clear();

                foreach (var node in jassProject.Nodes)
                {
                    NodeConfigEntity nodeConfig = new(NodeHandleId, node);
                    NodeConfigs.Add(NodeHandleId, nodeConfig);
                    IndexNodeSourceFile.Add(node.SourceFilePath, nodeConfig);
                    NodeHandleId++;
                }
            }

            public List<long> Remove(List<long> nodeHandleIds)
            {
                return nodeHandleIds.Where(nodeHandleId =>
                {
                    if (NodeConfigs.Remove(nodeHandleId))
                    {
                        IndexNodeSourceFile.Remove(NodeConfigs[nodeHandleId].SourceFilePath);
                        return true;
                    }
                    return false;
                }).ToList();
            }

            public List<long> Select(List<long> nodeHandleIds)
            {
                return nodeHandleIds.Where(NodeConfigs.ContainsKey).ToList();
            }

            public List<T> Select<T>(List<T> selectRequests) where T : INodeSelectRequest
            {
                return selectRequests.Where(selectReqeust => NodeConfigs.ContainsKey(selectReqeust.NodeHandleId)).ToList();
            }

            public List<NodeConfig> SelectAllBySourceFilePathIn(List<string> sourceFilePaths)
            {
                var selectedNodes = new List<NodeConfig>();

                foreach (var sourceFilePath in sourceFilePaths.Distinct())
                {
                    if (IndexNodeSourceFile.TryGetValue(sourceFilePath, out var nodeConfig))
                    {
                        selectedNodes.Add(nodeConfig.Snapshot());
                    }
                }

                return selectedNodes;
            }
        }

        public class NodeConfig(long nodeHandleId, int x, int y, int width, int height, string sourceFilePath, BitmapImage? image)
        {
            public long NodeHandleId { get; private set; } = nodeHandleId;
            public int X { get; private set; } = x;
            public int Y { get; private set; } = y;
            public int Width { get; private set; } = width;
            public int Height { get; private set; } = height;
            public string SourceFilePath { get; private set; } = sourceFilePath;
            public BitmapImage? Image { get; private set; } = image;
        }

        internal class NodeConfigEntity(long nodeHandleId, JassProject.Node node)
        {
            public long NodeHandleId = nodeHandleId;
            public int X = node.X;
            public int Y = node.Y;
            public int Width = node.Width;
            public int Height = node.Height;
            public string SourceFilePath = node.SourceFilePath;
            public BitmapImage? Image = Base64Helper.Convert(node.ImageBase64String);

            public JassProject.Node Cast()
            {
                return new JassProject.Node(X, Y, Width, Height, SourceFilePath, Base64Helper.Convert(Image));
            }

            public NodeConfig Snapshot()
            {
                return new NodeConfig(NodeHandleId, X, Y, Width, Height, SourceFilePath, Image);
            }
        }
    }
}
