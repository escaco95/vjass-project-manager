using System.Windows;
using vJassMainJBlueprint.Utils;

namespace vJassMainJBlueprint.V1
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // FeatureManager를 통해 기능 초기화
            this.InitializeFeatures();
        }
    }
}
