using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using vJassMainJBlueprint.Utils;

namespace vJassMainJBlueprint.V1
{
    partial class MainWindow
    {
        /// <summary>
        /// 창의 레이아웃을 최적화하는 기능을 제공하는 클래스.
        /// 창 크기 변경 시 발생하는 불필요한 레이아웃 재계산을 줄이고 성능을 개선합니다.
        /// </summary>
        [FeatureManager.FeatureSubscriber]
        private class PluginOptimizeLayout
        {
            // 200ms 딜레이 후 레이아웃 업데이트 (자주 발생하는 레이아웃 계산을 줄이기 위해 설정)
            private static readonly TimeSpan LayoutUpdateDelay = TimeSpan.FromMilliseconds(200);

            /// <summary>
            /// 클래스 생성자. MainWindow의 레이아웃 최적화를 초기화합니다.
            /// </summary>
            private PluginOptimizeLayout()
            {
                FeatureManager.SubscribeToFeature<MainWindow>(InitializeFeature);
            }

            /// <summary>
            /// MainWindow의 레이아웃 최적화 기능을 초기화하는 메소드.
            /// </summary>
            /// <param name="mainWindow">최적화할 대상 MainWindow</param>
            private static void InitializeFeature(MainWindow mainWindow)
            {
                // 창 크기 변경 시 깜박임 현상을 방지하기 위해 배경을 검은색으로 설정하고,
                // 레이아웃 재계산을 최소화하기 위한 최적화 적용
                OptimizeVisualTemplate(mainWindow, mainWindow.ClientArea, mainWindow.FooterContainer);

                // 창 크기 변경 시 최적화된 레이아웃 동작을 적용
                OptimizeLayoutBehavior(mainWindow, mainWindow.ClientArea, mainWindow.FooterContainer);
            }

            /// <summary>
            /// 창 크기 변경 시 깜박임을 방지하고 레이아웃 재계산을 줄이기 위한 시각적 최적화를 적용합니다.
            /// </summary>
            /// <param name="window">최적화할 창</param>
            /// <param name="clientArea">클라이언트 영역</param>
            /// <param name="footerContainer">하단 컨테이너</param>
            private static void OptimizeVisualTemplate(Window window, FrameworkElement clientArea, FrameworkElement footerContainer)
            {
                // 창의 배경을 검은색으로 설정하여 크기 변경 시 깜박임 방지
                window.Background = Brushes.Black;

                // 레이아웃 계산을 줄이기 위해 footerContainer를 시각적 트리에서 분리
                footerContainer.ClipToBounds = true;
                footerContainer.Width = clientArea.ActualWidth;
                footerContainer.Height = clientArea.ActualHeight;
                footerContainer.HorizontalAlignment = HorizontalAlignment.Left;
                footerContainer.VerticalAlignment = VerticalAlignment.Top;
            }

            /// <summary>
            /// 창 크기 변경 시 최적화된 레이아웃 동작을 적용하여 불필요한 레이아웃 재계산을 방지합니다.
            /// </summary>
            /// <param name="window">최적화할 창</param>
            /// <param name="clientArea">클라이언트 영역</param>
            /// <param name="footerContainer">하단 컨테이너</param>
            private static void OptimizeLayoutBehavior(Window window, FrameworkElement clientArea, FrameworkElement footerContainer)
            {
                // 타이머 설정 (크기 변경 후 200ms 후에 레이아웃을 재계산)
                DispatcherTimer resizeTimer = new() { Interval = LayoutUpdateDelay };

                // 창의 크기가 변경될 때 이벤트 핸들러 등록
                window.SizeChanged += (sender, e) =>
                {
                    // 창 크기가 변경되면 타이머 시작 (또는 리셋)
                    if (!resizeTimer.IsEnabled)
                    {
                        resizeTimer.Start();
                    }
                };

                // 타이머 틱 이벤트 발생 시 레이아웃을 업데이트
                resizeTimer.Tick += (sender, e) =>
                {
                    // UI 스레드에서 레이아웃을 업데이트
                    window.Dispatcher.Invoke(() =>
                    {
                        double clientWidth = clientArea.ActualWidth;
                        double clientHeight = clientArea.ActualHeight;

                        // 클라이언트 영역과 하단 컨테이너의 크기가 다를 경우에만 업데이트 (불필요한 재계산 방지)
                        if (Math.Abs(footerContainer.ActualWidth - clientWidth) > 0.1 ||
                            Math.Abs(footerContainer.ActualHeight - clientHeight) > 0.1)
                        {
                            footerContainer.Width = clientWidth;
                            footerContainer.Height = clientHeight;
                        }
                        else
                        {
                            // 크기가 제대로 적용되면 타이머 중지
                            resizeTimer.Stop();
                        }
                    });
                };
            }
        }
    }
}
