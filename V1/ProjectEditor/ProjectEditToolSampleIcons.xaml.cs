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
    public partial class ProjectEditToolSampleIcons : UserControl
    {
        public ProjectEditToolSampleIcons()
        {
            InitializeComponent();
            LoadSampleIcons();
        }

        private void LoadSampleIcons()
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

                image.MouseDown += Image_MouseDown;
                RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);

                SampleIconContainer.Children.Add(image);
            }
        }

        private void Image_MouseDown(object sender, RoutedEventArgs e)
        {
            if (sender is Image image)
            {
                Clipboard.SetImage(image.Source as BitmapSource);
                Messenger.Send(new ProjectEditElementMessageBox.ShowActionMessage("아이콘을 복사했습니다"));
            }
        }
    }
}
