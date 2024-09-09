using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using vJassMainJBlueprint.Utils;
using vJassMainJBlueprint.V1.ModelFacade;

namespace vJassMainJBlueprint.V1.ProjectEditor
{
    /// <summary>
    /// ProjectEditWorkspace.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ProjectEditWorkspace : UserControl
    {
        // 편집기 파사드
        private readonly ProjectEditFacade projectEditFacade = new();
        private readonly Dictionary<long, ProjectEditNode> nodeElements = new();

        public ProjectEditWorkspace()
        {
            InitializeComponent();

            // 편집기 파사드 초기화
            projectEditFacade.UpdateRequired += OnEditFacadeUpdateRequired;
        }

        private void OnEditFacadeUpdateRequired(ProjectEditFacade sender, ProjectEditFacade.IUpdateRequiredEventArgs[] args)
        {
            foreach (var arg in args)
            {
                switch (arg.EventType)
                {
                    case ProjectEditFacade.EventType.PROJECT_ORIGIN_UPDATE:
                        OnProjectOriginChanged((ProjectEditFacade.ProjectOriginUpdateEventArgs)arg);
                        break;
                    case ProjectEditFacade.EventType.PROJECT_ORIGIN_SAVE_REQUIRE:
                        OnProjectSaveRequiredChanged((ProjectEditFacade.ProjectOriginSaveRequireEventArgs)arg);
                        break;
                    case ProjectEditFacade.EventType.PROJECT_NAME_CHANGED:
                        OnProjectNameChanged((ProjectEditFacade.ProjectNameChangedEventArgs)arg);
                        break;
                    case ProjectEditFacade.EventType.PROJECT_AUTHOR_CHANGED:
                        OnProjectAuthorChanged((ProjectEditFacade.ProjectAuthorChangedEventArgs)arg);
                        break;
                    case ProjectEditFacade.EventType.PROJECT_RESIZE:
                        OnProjectSizeChanged((ProjectEditFacade.ProjectResizeEventArgs)arg);
                        break;
                    case ProjectEditFacade.EventType.NODE_ADD:
                        OnProjectNodeAdded((ProjectEditFacade.NodeAddEventArgs)arg);
                        break;
                    case ProjectEditFacade.EventType.NODE_REMOVE:
                        OnProjectNodeRemoved((ProjectEditFacade.NodeRemoveEventArgs)arg);
                        break;
                    case ProjectEditFacade.EventType.NODE_BOUND_UPDATE:
                        OnProjectNodeBoundChanged((ProjectEditFacade.NodeBoundUpdateEventArgs)arg);
                        break;
                    case ProjectEditFacade.EventType.NODE_SOURCE_FILE_PATH_UPDATE:
                        OnProjectNodeSourceFilePathChanged((ProjectEditFacade.NodeSourceFilePathUpdateEventArgs)arg);
                        break;
                    case ProjectEditFacade.EventType.NODE_IMAGE_UPDATE:
                        OnProjectNodeImageChanged((ProjectEditFacade.NodeImageUpdateEventArgs)arg);
                        break;
                }
            }
        }

        private void OnProjectOriginChanged(ProjectEditFacade.ProjectOriginUpdateEventArgs e)
        {
            Optional<Window>.Of(Window.GetWindow(this)).IfPresent(window =>
            {
                window.Title = "vJass Project Manager" + (e.OriginType == ProjectEditFacade.OriginType.Memory ? " - 새 프로젝트" : " - " + e.OriginFilePath) + (window.Title.Contains('*') ? "*" : "");
            });
        }

        private void OnProjectSaveRequiredChanged(ProjectEditFacade.ProjectOriginSaveRequireEventArgs e)
        {
            Optional<Window>.Of(Window.GetWindow(this)).IfPresent(window =>
            {
                window.Title = window.Title.Replace("*", "") + (e.SaveRequired ? "*" : "");
            });
        }

        private void OnProjectNameChanged(ProjectEditFacade.ProjectNameChangedEventArgs arg)
        {
            Dispatcher.Invoke(() =>
            {
                TextProjectName.Text = arg.Name;
            });
        }

        private void OnProjectAuthorChanged(ProjectEditFacade.ProjectAuthorChangedEventArgs arg)
        {
            Dispatcher.Invoke(() =>
            {
                TextProjectAuthor.Text = arg.Author;
            });
        }

        private void OnProjectSizeChanged(ProjectEditFacade.ProjectResizeEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                FooterZoomGrid.Width = e.Width;
                FooterZoomGrid.Height = e.Height;
            });
        }

        private void OnProjectNodeBoundChanged(ProjectEditFacade.NodeBoundUpdateEventArgs e)
        {
            Dispatcher.Invoke(() => { nodeElements[e.NodeHandleId].UpdateNode(e); });
        }

        private void OnProjectNodeSourceFilePathChanged(ProjectEditFacade.NodeSourceFilePathUpdateEventArgs e)
        {
            Dispatcher.Invoke(() => { nodeElements[e.NodeHandleId].UpdateNode(e); });
        }

        private void OnProjectNodeImageChanged(ProjectEditFacade.NodeImageUpdateEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Dispatcher.Invoke(() => { nodeElements[e.NodeHandleId].UpdateNode(e); });
            });
        }

        private void OnProjectNodeRemoved(ProjectEditFacade.NodeRemoveEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                // 물리 노드 제거
                NodeContainer.Children.Remove(nodeElements[e.NodeHandleId]);
                // 논리 노드 제거
                nodeElements.Remove(e.NodeHandleId);
            });
        }

        private void OnProjectNodeAdded(ProjectEditFacade.NodeAddEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ProjectEditNode nodeElement = new(e);
                // 논리 노드 추가
                nodeElements.Add(e.NodeHandleId, nodeElement);
                // 물리 노드 추가
                NodeContainer.Children.Add(nodeElement);
            });
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // UserControl이 로드될 때 포커스 설정
            Keyboard.Focus(this);

            // 편집기 파사드 기본 동작 - 새 문서 생성
            projectEditFacade.MakeNewProject();
            // 부모 윈도우 닫기 동작 후킹
            Optional<Window>.Of(Window.GetWindow(this)).IfPresent(window =>
            {
                window.Closing += (sender, e) =>
                {
                    SafeAction(() =>
                    {
                        if (HandleSaveChanges())
                        {
                            e.Cancel = true;
                            return;
                        }

                        projectEditFacade.MakeNewProject();
                    });
                };
            });
            // 화면 초기화
            ViewportResetDelayed();
        }

        private void SafeAction(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Window.GetWindow(this), ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
