using System.IO;
using System.Windows;
using vJassMainJBlueprint.Utils;
using vJassMainJBlueprint.V1.ModelFacade;
using vJassMainJBlueprint.V1.ModelHelper;

namespace vJassMainJBlueprint.V1.ProjectEditor
{
    partial class ProjectEditWorkspace
    {
        // 새 프로젝트 생성 메서드
        private void OnMenuFileNewClick(object? sender, RoutedEventArgs? e)
        {
            SafeAction(() =>
            {
                if (HandleSaveChanges()) return;

                projectEditFacade.MakeNewProject();
                ViewportResetDelayed(true);

                MessageText.Info("새 프로젝트를 생성했습니다.");
            });
        }

        // 프로젝트 열기 메서드
        private void OnMenuFileOpenClick(object? sender, RoutedEventArgs? e)
        {
            SafeAction(() =>
            {
                if (HandleSaveChanges()) return;

                Optional<string>.Of(JassProjectOpenFileDialog.Show())
                .IfPresent(filePath =>
                {
                    projectEditFacade.MakeNewProject(filePath, JassProjectReader.Read(filePath));
                    ViewportResetDelayed(true);

                    MessageText.Info($"{Path.GetFileName(filePath)} 프로젝트를 열었습니다.");
                });
            });
        }

        // 프로젝트 닫기 메서드
        private void OnMenuFileCloseClick(object? sender, RoutedEventArgs? e)
        {
            SafeAction(() =>
            {
                if (HandleSaveChanges()) return;

                projectEditFacade.MakeNewProject();
                ViewportResetDelayed(true);

                MessageText.Info("프로젝트를 닫았습니다.");
            });
        }

        // 프로젝트 저장 메서드
        private void OnMenuFileSaveClick(object? sender, RoutedEventArgs? e)
        {
            SafeAction(() =>
            {
                Optional<bool>.Of(projectEditFacade.OriginSaveRequired)
                    .Filter(saveRequired => saveRequired)
                    .IfPresent(_ =>
                    {
                        if (HandleSaveProject()) return;

                        MessageText.Info("저장했습니다.");
                    });
            });
        }

        // 다른 이름으로 프로젝트 저장 메서드
        private void OnMenuFileSaveAsClick(object? sender, RoutedEventArgs? e)
        {
            SafeAction(() =>
            {
                Optional<string>.Of(JassProjectSaveFileDialog.Show())
                .IfPresent(filePath =>
                {
                    JassProjectWriter.Write(projectEditFacade.GetProject(), filePath);
                    projectEditFacade.UpdateOrigin(filePath);

                    MessageText.Info($"{Path.GetFileName(filePath)} 파일로 저장했습니다.");
                });
            });
        }

        // 프로그램 종료 메서드
        private void OnMenuFileExitClick(object sender, RoutedEventArgs e)
        {
            // 이미 창 닫기 이벤트에서 저장 여부를 확인하므로 여기서는 확인하지 않음
            Application.Current.Shutdown();
        }

        // 변경사항 저장 여부 확인 및 처리 메서드
        private bool HandleSaveChanges()
        {
            if (projectEditFacade.OriginSaveRequired)
            {
                switch (MessageBox.Show(Window.GetWindow(this), "저장되지 않은 변경사항이 있습니다. 저장하시겠습니까?", "저장", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                {
                    case MessageBoxResult.Cancel:
                        return true;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Yes:
                        HandleSaveProject();
                        break;
                }
            }
            return false;
        }

        // 프로젝트 저장 메서드
        private bool HandleSaveProject()
        {
            if (projectEditFacade.Origin == ProjectEditFacade.OriginType.File)
            {
                JassProjectWriter.Write(projectEditFacade.GetProject(), projectEditFacade.OriginFilePath);
                projectEditFacade.UpdateOriginSaveRequired(false);
                return false;
            }
            else
            {
                if (JassProjectSaveFileDialog.Show() is string filePath)
                {
                    JassProjectWriter.Write(projectEditFacade.GetProject(), filePath);
                    projectEditFacade.UpdateOrigin(filePath);
                    return false;
                }
                return true;
            }
        }
    }
}
