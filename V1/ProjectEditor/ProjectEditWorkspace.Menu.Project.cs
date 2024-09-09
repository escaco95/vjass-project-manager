using System.Windows;
using vJassMainJBlueprint.Utils;
using vJassMainJBlueprint.V1.Config;

namespace vJassMainJBlueprint.V1.ProjectEditor
{
    partial class ProjectEditWorkspace
    {
        private void OnMenuProjectChangeName(object? sender, RoutedEventArgs? e)
        {
            string oldProjectName = projectEditFacade.GetProjectName();

            if (InputBox.Show(Window.GetWindow(this), "프로젝트 이름 바꾸기", "새로운 프로젝트 이름을 입력하세요.", oldProjectName) is not string newProjectName) return;

            if (projectEditFacade.UpdateProjectName(newProjectName))
            {
                projectEditFacade.UpdateOriginSaveRequired(true);

                MessageText.Info($"프로젝트 이름을 '{newProjectName}'로 변경했습니다.");
            }
        }

        private void OnMenuProjectChangeAuthor(object? sender, RoutedEventArgs? e)
        {
            string oldProjectAuthor = projectEditFacade.GetProjectAuthor();

            if (InputBox.Show(Window.GetWindow(this), "프로젝트 저자 바꾸기", "새로운 프로젝트 저자를 입력하세요.", oldProjectAuthor) is not string newProjectAuthor) return;

            if (projectEditFacade.UpdateProjectAuthor(newProjectAuthor))
            {
                projectEditFacade.UpdateOriginSaveRequired(true);

                MessageText.Info($"프로젝트 저자를 '{newProjectAuthor}'로 변경했습니다.");
            }
        }

        private void OnMenuProjectChangeSize(object? sender, RoutedEventArgs? e)
        {
            string oldProjectSize = $"{projectEditFacade.GetProjectWidth()},{projectEditFacade.GetProjectHeight()}";

            if (InputBox.Show(Window.GetWindow(this), "프로젝트 크기 바꾸기", "새로운 프로젝트 크기를 입력하세요.\n(가로,세로)", oldProjectSize) is not string newProjectSize) return;

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
    }
}
