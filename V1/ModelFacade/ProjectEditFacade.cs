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
            public Dictionary<long, NodeConfig> NodeConfigs = [];

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
                    NodeConfigs.Add(NodeHandleId, new NodeConfig(NodeHandleId, node));
                    NodeHandleId++;
                }
            }

            public List<long> Remove(List<long> nodeHandleIds)
            {
                return nodeHandleIds.Where(NodeConfigs.Remove).ToList();
            }

            public List<long> Select(List<long> nodeHandleIds)
            {
                return nodeHandleIds.Where(NodeConfigs.ContainsKey).ToList();
            }

            public List<T> Select<T>(List<T> selectRequests) where T : INodeSelectRequest
            {
                return selectRequests.Where(selectReqeust => NodeConfigs.ContainsKey(selectReqeust.NodeHandleId)).ToList();
            }
        }

        internal class NodeConfig(long nodeHandleId, JassProject.Node node)
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
        }
    }
}
