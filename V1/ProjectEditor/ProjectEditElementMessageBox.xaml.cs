using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace vJassMainJBlueprint.V1.ProjectEditor
{
    /// <summary>
    /// ProjectEditElementMessageBox.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ProjectEditElementMessageBox : UserControl
    {
        private readonly TimeSpan fadeDuration = TimeSpan.FromSeconds(0.5); // 페이드 아웃 지속 시간
        private readonly TimeSpan visibleDuration = TimeSpan.FromSeconds(0.5); // 메시지가 보이는 시간
        private readonly DispatcherTimer collapseTimer = new ();

        public ProjectEditElementMessageBox()
        {
            InitializeComponent();

            collapseTimer.Interval = TimeSpan.FromSeconds(1);
            collapseTimer.Tick += (sender, e) =>
            {
                collapseTimer.Stop();
                Visibility = System.Windows.Visibility.Collapsed;
            };
        }

        // 메시지 표시 후 페이드 아웃 애니메이션을 실행하는 메서드
        private void ShowMessage(string message, Brush backgroundColor)
        {
            MessageTextBlock.Text = message;
            BlockBackdrop.Background = backgroundColor;

            // 이전 애니메이션이 있으면 중지
            BlockRoot.BeginAnimation(OpacityProperty, null);

            // 메시지 표시
            Visibility = System.Windows.Visibility.Visible;
            BlockRoot.Opacity = 1;

            // 페이드 아웃 애니메이션 초기화 및 설정
            DoubleAnimation fadeOutAnimation = new(0, fadeDuration)
            {
                BeginTime = visibleDuration
            };

            // 페이드 아웃 애니메이션 완료 후 메시지 숨김
            collapseTimer.Stop();
            collapseTimer.Start();

            // 페이드 아웃 애니메이션 (잠시 후 실행)
            BlockRoot.BeginAnimation(OpacityProperty, fadeOutAnimation);
        }

        // Info 메시지 표시
        public void Info(string message)
        {
            ShowMessage(message, Brushes.Blue);
        }

        // Warn 메시지 표시
        public void Warn(string message)
        {
            ShowMessage(message, Brushes.Orange);
        }

        // Error 메시지 표시
        public void Error(string message)
        {
            ShowMessage(message, Brushes.Red);
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            BlockRoot.Opacity = 0;
        }
    }
}
