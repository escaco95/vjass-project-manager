using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using vJassMainJBlueprint.Utils;
using vJassMainJBlueprint.V1.Config;
using vJassMainJBlueprint.V1.ModelFacade;

namespace vJassMainJBlueprint.V1.ProjectEditor
{
    partial class ProjectEditWorkspace
    {
        private void OnMenuProjectChangeName(object? sender, RoutedEventArgs? e)
        {
            string oldProjectName = projectEditFacade.GetProjectName();

            if (InputBox.Show(this, "프로젝트 이름 바꾸기", "새로운 프로젝트 이름을 입력하세요.", oldProjectName) is not string newProjectName) return;

            if (projectEditFacade.UpdateProjectName(newProjectName))
            {
                projectEditFacade.UpdateOriginSaveRequired(true);

                MessageText.Info($"프로젝트 이름을 '{newProjectName}'로 변경했습니다.");
            }
        }

        private void OnMenuProjectChangeAuthor(object? sender, RoutedEventArgs? e)
        {
            string oldProjectAuthor = projectEditFacade.GetProjectAuthor();

            if (InputBox.Show(this, "프로젝트 저자 바꾸기", "새로운 프로젝트 저자를 입력하세요.", oldProjectAuthor) is not string newProjectAuthor) return;

            if (projectEditFacade.UpdateProjectAuthor(newProjectAuthor))
            {
                projectEditFacade.UpdateOriginSaveRequired(true);

                MessageText.Info($"프로젝트 저자를 '{newProjectAuthor}'로 변경했습니다.");
            }
        }

        private void OnMenuProjectChangeSize(object? sender, RoutedEventArgs? e)
        {
            string oldProjectSize = $"{projectEditFacade.GetProjectWidth()},{projectEditFacade.GetProjectHeight()}";

            if (InputBox.Show(this, "프로젝트 크기 바꾸기", "새로운 프로젝트 크기를 입력하세요.\n(가로,세로)", oldProjectSize) is not string newProjectSize) return;

            // 파싱 가능한 양식으로 입력했는지 검증
            if (newProjectSize.Split(',').Length != 2 || !int.TryParse(newProjectSize.Split(',')[0], out int newWidth) || !int.TryParse(newProjectSize.Split(',')[1], out int newHeight))
            {
                MessageBox.Show(Window.GetWindow(this), "쉼표로 구분되는 가로,세로 크기를 입력해야 합니다.", "프로젝트 크기 바꾸기", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 허용되는 크기인지 검증
            if (!JassProjectSpecification.IsAllowedProjectSize(newWidth, newHeight))
            {
                MessageBox.Show(Window.GetWindow(this), $"허용되지 않는 프로젝트 크기입니다.\n(가로,세로) {JassProjectSpecification.MinProjectSize} 이상 {JassProjectSpecification.MaxProjectSize} 이하로 입력하세요.", "프로젝트 크기 바꾸기", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (projectEditFacade.UpdateProjectSize(newWidth, newHeight))
            {
                projectEditFacade.UpdateOriginSaveRequired(true);

                MessageText.Info($"프로젝트를 {newWidth}x{newHeight} 크기로 변경했습니다.");
            }
        }

        private void OnMenuProjectSourceAdd(object? sender, RoutedEventArgs? e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Multiselect = true,
                Filter = "모든 파일|*.*",
                Title = "설계도에 추가할 파일 모두 선택",
            };

            if (openFileDialog.ShowDialog() != true) return;
            if (openFileDialog.FileNames.Length == 0) return;

            ProjectSourceAdd([.. openFileDialog.FileNames]);
        }

        private void OnMenuProjectSourceAddDirectory(object? sender, RoutedEventArgs? e)
        {
            OpenFolderDialog openFolderDialog = new()
            {
                Multiselect = true,
                Title = "설계도에 추가할 디렉토리 모두 선택",
            };

            if (openFolderDialog.ShowDialog() != true) return;
            if (openFolderDialog.FolderNames.Length == 0) return;

            List<string> filePaths = [];
            try
            {
                filePaths.AddRange(openFolderDialog.FolderNames.ToList().SelectMany(Directory.GetFiles).Distinct());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageText.Error("디렉토리 파일 스캔 중 오류가 발생했습니다.");
                return;
            }

            ProjectSourceAdd(filePaths);
        }

        private void ProjectSourceAdd(List<string> filePaths)
        {
            // 노드의 추가 위치는 화면 중앙
            double zoomFactor = ((ScaleTransform)ZoomChild.LayoutTransform).ScaleX;
            var viewX = ZoomParent.ActualWidth / 2 / zoomFactor;
            var viewY = ZoomParent.ActualHeight / 2 / zoomFactor;
            var worldX = - Canvas.GetLeft(ZoomChild) / zoomFactor + viewX;
            var worldY = - Canvas.GetTop(ZoomChild) / zoomFactor + viewY;

            projectEditFacade.InsertNode(filePaths.Select(filePath =>
            {
                return new ProjectEditFacade.NodeAddRequest()
                {
                    X = (int)worldX,
                    Y = (int)worldY,
                    Width = 100,
                    Height = 100,
                    SourceFilePath = filePath,
                    Image = null,
                };
            }).ToList());
        }

        private void OnMenuProjectApplyRelativePaths(object? sender, RoutedEventArgs? e)
        {
            // 만약 프로젝트가 파일로 저장되지 않았다면 경로를 적용할 수 없음
            if (projectEditFacade.Origin != ProjectEditFacade.OriginType.File)
            {
                MessageText.Warn("프로젝트가 파일로 저장되지 않았습니다.");
                return;
            }
            var changedPaths = PathHelper.ConvertToRelativePaths(projectEditFacade.OriginFilePath, projectEditFacade.SelectNodeSourceFilePaths());
            if (changedPaths.Count == 0)
            {
                MessageText.Info("적용 가능한 모든 노드가 이미 상대 경로입니다.");
                return;
            }

            List<ProjectEditFacade.NodeSourceFilePathUpdateRequest> request = [];
            foreach (var (before, after) in changedPaths)
            {
                projectEditFacade.SelectNodeBySourceFilePath(before).ForEach(nodeConfig =>
                {
                    request.Add(new ProjectEditFacade.NodeSourceFilePathUpdateRequest
                    {
                        NodeHandleId = nodeConfig.NodeHandleId,
                        SourceFilePath = after,
                    });
                });
            }
            int updatedNodesCount = projectEditFacade.UpdateNodeSourceFilePath(request);
            projectEditFacade.UpdateOriginSaveRequired(true);

            MessageText.Info($"{updatedNodesCount} 노드를 상대 경로로 변경했습니다.");
        }
    }
}
