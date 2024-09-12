using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using vJassMainJBlueprint.Utils;

namespace vJassMainJBlueprint.V1
{
    partial class MainWindow
    {
        /// <summary>
        /// MainWindow의 상태를 저장하고 복원하는 기능을 담당하는 클래스.
        /// 이 클래스는 구독-발행 패턴을 통해 MainWindow의 생성 및 종료 시점에서 창의 상태를 관리합니다.
        /// </summary>
        [FeatureManager.FeatureSubscriber]
        private class PluginPreserveState
        {
            // JSON 직렬화 옵션 (보기 좋은 형식으로 출력)
            private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };

            // 상태를 저장할 파일 경로
            private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.window.json");

            // 임시 파일 경로 (상태 저장 중 문제 발생 시 대비)
            private static readonly string TempConfigFilePath = ConfigFilePath + ".temp";

            // 창 상태를 저장할 레코드 타입 (구조체 형태로 창의 상태를 보존)
            private record WindowStateConfig(bool IsMaximized, double Left, double Top, double Width, double Height, bool Topmost);

            /// <summary>
            /// 클래스 생성자. MainWindow의 생성 시 창 상태를 복원하고, 종료 시 상태를 저장합니다.
            /// </summary>
            private PluginPreserveState()
            {
                FeatureManager.SubscribeToFeature<MainWindow>(InitializeFeature);
            }

            /// <summary>
            /// MainWindow에 대한 창 상태 관리 기능을 초기화합니다.
            /// </summary>
            /// <param name="mainWindow">상태를 관리할 대상 MainWindow</param>
            private static void InitializeFeature(MainWindow mainWindow)
            {
                // 창이 로드되면 상태를 적용하고, 종료될 때 상태를 저장
                mainWindow.Loaded += (_, _) => ApplyWindowState(mainWindow);
                mainWindow.Closing += (_, _) => SaveWindowState(mainWindow);
            }

            /// <summary>
            /// 이전에 저장된 창 상태를 적용합니다.
            /// </summary>
            /// <param name="window">상태를 적용할 대상 Window</param>
            private static void ApplyWindowState(Window window)
            {
                if (!File.Exists(ConfigFilePath))
                    return;

                try
                {
                    var json = File.ReadAllText(ConfigFilePath);
                    var config = JsonSerializer.Deserialize<WindowStateConfig>(json);

                    if (config != null)
                    {
                        // 창의 위치 및 크기 복원
                        window.Left = config.Left;
                        window.Top = config.Top;
                        window.Width = config.Width;
                        window.Height = config.Height;
                        window.WindowState = config.IsMaximized ? WindowState.Maximized : WindowState.Normal;
                        window.Topmost = config.Topmost;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] 창 상태를 적용하는 중 오류가 발생했습니다: {ex.Message}");
                }
            }

            /// <summary>
            /// 현재 창의 상태를 저장합니다.
            /// </summary>
            /// <param name="window">상태를 저장할 대상 Window</param>
            public static void SaveWindowState(Window window)
            {
                var config = new WindowStateConfig(
                    IsMaximized: window.WindowState == WindowState.Maximized,
                    Left: window.Left,
                    Top: window.Top,
                    Width: window.Width,
                    Height: window.Height,
                    Topmost: window.Topmost
                );

                try
                {
                    // 창 상태를 JSON으로 직렬화
                    var json = JsonSerializer.Serialize(config, JsonSerializerOptions);

                    // 임시 파일에 쓰기
                    File.WriteAllText(TempConfigFilePath, json);

                    // 기존 파일이 있으면 교체, 없으면 이동
                    if (File.Exists(ConfigFilePath))
                    {
                        File.Replace(TempConfigFilePath, ConfigFilePath, null);
                    }
                    else
                    {
                        File.Move(TempConfigFilePath, ConfigFilePath);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERROR] 창 상태를 저장하는 중 오류가 발생했습니다: {ex.Message}");
                }
                finally
                {
                    // 임시 파일 삭제
                    if (File.Exists(TempConfigFilePath))
                    {
                        File.Delete(TempConfigFilePath);
                    }
                }
            }
        }
    }
}
