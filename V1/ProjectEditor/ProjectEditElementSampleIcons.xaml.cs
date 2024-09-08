using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using vJassMainJBlueprint.Utils;
using vJassMainJBlueprint.V1.Config;

namespace vJassMainJBlueprint.V1.ProjectEditor
{
    /// <summary>
    /// ProjectEditElementSampleIcons.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ProjectEditElementSampleIcons : UserControl
    {
        public ProjectEditElementSampleIcons()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var sampleImageBase64String in JassProjectSpecification.SampleImages)
            {
                Image image = new()
                {
                    Width = 20,
                    Height = 20,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 0, 0, 10),
                    Source = Base64Helper.Convert(sampleImageBase64String),
                };

                image.MouseDown += (sender, e) =>
                {
                    Clipboard.SetImage(image.Source as BitmapSource);
                    MessageBox.Show("아이콘을 복사했습니다!", "샘플 아이콘", MessageBoxButton.OK, MessageBoxImage.Information);
                };

                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);

                SampleIconFooter.Children.Add(image);
            }
        }
    }
}
