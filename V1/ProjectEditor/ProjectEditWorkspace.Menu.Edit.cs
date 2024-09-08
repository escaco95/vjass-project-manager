using System.Windows;
using System.Windows.Media.Imaging;
using vJassMainJBlueprint.Utils;

namespace vJassMainJBlueprint.V1.ProjectEditor
{
    partial class ProjectEditWorkspace
    {
        private void OnMenuEditDelete(object? sender, RoutedEventArgs? e)
        {
            List<long> deleteTargets = [.. selectedNodeIds.Keys];
            SelectionClear();
            if (deleteTargets.Count == 0)
            {
                MessageText.Warn("삭제할 노드가 없습니다.");
                return;
            }

            projectEditFacade.DeleteNodes(deleteTargets);
            projectEditFacade.UpdateOriginSaveRequired(true);

            MessageText.Info("선택한 노드를 삭제했습니다.");
        }

        private void OnMenuEditSelectAll(object? sender, RoutedEventArgs? e)
        {
            SelectionAdd(NodeContainer.Children.OfType<ProjectEditNode>().Select(node => node.SourceNodeID).ToList());
            
            if(selectedNodeIds.Count == 0)
            {
                MessageText.Warn("선택할 노드가 없습니다.");
            }
            else
            {
                MessageText.Info("모든 노드를 선택했습니다.");
            }
        }

        private void OnMenuEditUnselectAll(object? sender, RoutedEventArgs? e)
        {
            SelectionClear();
        }

        private void OnMenuEditCopyImage(object? sender, RoutedEventArgs? e)
        {
            if (selectedNodeIds.Count == 0)
            {
                MessageText.Warn("이미지를 복사할 노드를 선택하지 않았습니다.");
                return;
            }
            if (selectedNodeIds.Count > 1)
            {
                MessageText.Warn("이미지를 복사할 노드를 하나만 선택해주세요.");
                return;
            }
            if (projectEditFacade.SelectNodeImage(selectedNodeIds.Keys.First()) is not BitmapImage image)
            {
                MessageText.Warn("이미지가 없는 노드는 복사할 수 없습니다.");
                return;
            }
            Clipboard.SetImage(image);
            MessageText.Info("이미지가 복사되었습니다.");
        }

        private void OnMenuEditPasteImage(object? sender, RoutedEventArgs? e)
        {
            if (!Clipboard.ContainsImage())
            {
                MessageText.Warn("클립보드에 이미지가 없습니다.");
                return;
            }
            if (selectedNodeIds.Count == 0)
            {
                MessageText.Warn("이미지를 붙여넣을 노드를 선택해주세요.");
                return;
            }
            if (Base64Helper.Convert(Clipboard.GetImage()) is not string image)
            {
                MessageText.Warn("프로그램에서 지원하지 않는 이미지 형식입니다.");
                return;
            }
            projectEditFacade.UpdateNodeImage([.. selectedNodeIds.Keys], Base64Helper.Convert(image));
            projectEditFacade.UpdateOriginSaveRequired(true);
            MessageText.Info("클립보드 이미지를 적용했습니다.");
        }
    }
}
