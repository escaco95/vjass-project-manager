using System.Windows.Media.Imaging;

namespace vJassMainJBlueprint.V1.ModelFacade
{
    partial class ProjectEditFacade
    {
        public string? SelectNodeSourceFilePath(long? nodeHandleId)
        {
            if (!nodeHandleId.HasValue) return null;
            if (!_nodeCollectionConfigs.NodeConfigs.TryGetValue(nodeHandleId.Value, out NodeConfig? nodeConfig)) return null;

            return nodeConfig.SourceFilePath;
        }

        public BitmapImage? SelectNodeImage(long? nodeHandleId)
        {
            if (!nodeHandleId.HasValue) return null;
            if (!_nodeCollectionConfigs.NodeConfigs.TryGetValue(nodeHandleId.Value, out NodeConfig? nodeConfig)) return null;

            return nodeConfig.Image;
        }

        public void DeleteNode(long nodeHandleId) => DeleteNodes([nodeHandleId]);

        public void DeleteNodes(List<long> nodeHandleIds)
        {
            List<NodeRemoveEventArgs> deletedNodeEventArgs = _nodeCollectionConfigs.Remove(nodeHandleIds)
                    .Select(nodeHandleId => new NodeRemoveEventArgs(nodeHandleId)).ToList();

            if (deletedNodeEventArgs.Count == 0) return;

            UpdateRequired?.Invoke(this, [.. deletedNodeEventArgs]);
        }

        public interface INodeSelectRequest
        {
            long NodeHandleId { get; }
        }

        public struct NodePositionUpdateRequest : INodeSelectRequest
        {
            public long NodeHandleId { get; set; }
            public int X;
            public int Y;
        }

        public void UpdateNodePosition(NodePositionUpdateRequest request) => UpdateNodePosition([request]);

        public void UpdateNodePosition(List<NodePositionUpdateRequest> requests)
        {
            UpdateNode(requests, (request) =>
            {
                NodeConfig selectedNodeConfig = _nodeCollectionConfigs.NodeConfigs[request.NodeHandleId];

                selectedNodeConfig.X = request.X;
                selectedNodeConfig.Y = request.Y;

                return new NodeImageUpdateEventArgs(selectedNodeConfig);
            });
        }

        public struct NodeSizeUpdateRequest : INodeSelectRequest
        {
            public long NodeHandleId { get; set; }
            public int Width;
            public int Height;
        }

        public void UpdateNodeSize(NodeSizeUpdateRequest request) => UpdateNodeSize([request]);

        public void UpdateNodeSize(List<NodeSizeUpdateRequest> requests)
        {
            UpdateNode(requests, (request) =>
            {
                NodeConfig selectedNodeConfig = _nodeCollectionConfigs.NodeConfigs[request.NodeHandleId];

                selectedNodeConfig.Width = request.Width;
                selectedNodeConfig.Height = request.Height;

                return new NodeImageUpdateEventArgs(selectedNodeConfig);
            });
        }

        public struct NodeImageUpdateRequest : INodeSelectRequest
        {
            public long NodeHandleId { get; set; }
            public BitmapImage? Image;
        }

        public void UpdateNodeImage(NodeImageUpdateRequest request) => UpdateNodeImage([request]);

        public void UpdateNodeImage(List<NodeImageUpdateRequest> requests)
        {
            UpdateNode(requests, (request) =>
            {
                NodeConfig selectedNodeConfig = _nodeCollectionConfigs.NodeConfigs[request.NodeHandleId];

                selectedNodeConfig.Image = request.Image;

                return new NodeImageUpdateEventArgs(selectedNodeConfig);
            });
        }

        public void UpdateNodeImage(long nodeHandleId, BitmapImage? image) => UpdateNodeImage([nodeHandleId], image);

        public void UpdateNodeImage(List<long> nodeHandleIds, BitmapImage? image)
        {
            List<long> selectedNodeIds = _nodeCollectionConfigs.Select(nodeHandleIds);

            if (selectedNodeIds.Count == 0) return;

            List<NodeImageUpdateEventArgs> updateEventArgs = selectedNodeIds.Select(nodeHandleId =>
            {
                NodeConfig selectedNodeConfig = _nodeCollectionConfigs.NodeConfigs[nodeHandleId];

                selectedNodeConfig.Image = image;

                return new NodeImageUpdateEventArgs(selectedNodeConfig);
            }).ToList();

            UpdateRequired?.Invoke(this, [.. updateEventArgs]);
        }

        private void UpdateNode<T, A>(List<T> requests, Func<T, A> requestFunc) where T : INodeSelectRequest where A : IUpdateRequiredEventArgs
        {
            List<T> selectedReqeusts = _nodeCollectionConfigs.Select(requests);

            if (selectedReqeusts.Count == 0) return;

            List<A> updateEventArgs = selectedReqeusts.Select(requestFunc.Invoke).ToList();

            UpdateRequired?.Invoke(this, [.. updateEventArgs]);
        }
    }
}
