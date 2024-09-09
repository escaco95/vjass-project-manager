using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using vJassMainJBlueprint.Utils;
using static vJassMainJBlueprint.V1.ModelFacade.ProjectEditFacade;

namespace vJassMainJBlueprint.V1.ProjectEditor
{
    /// <summary>
    /// ProjectEditNode.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ProjectEditNode : UserControl
    {

        public long SourceNodeID { get; private set; }

        public ProjectEditNode()
        {
            InitializeComponent();
        }

        public ProjectEditNode(NodeAddEventArgs e) : this()
        {
            SourceNodeID = e.NodeHandleId;

            UpdateBounds(e.X, e.Y, e.Width, e.Height);
            UpdateSourceFilePath(e.SourceFilePath);
            UpdateImage(e.Image);
        }

        public void UpdateNode(NodeBoundUpdateEventArgs e) => UpdateBounds(e.X, e.Y, e.Width, e.Height);

        public void UpdateNode(NodeSourceFilePathUpdateEventArgs e) => UpdateSourceFilePath(e.SourceFilePath);

        public void UpdateNode(NodeImageUpdateEventArgs e) => UpdateImage(e.Image);

        private void UpdateBounds(int x, int y, int width, int height)
        {
            Canvas.SetLeft(this, x);
            Canvas.SetTop(this, y);
            Width = width;
            Height = height;
        }

        private void UpdateSourceFilePath(string sourceFilePath)
        {
            if (sourceFilePath.StartsWith("..\\"))
            {
                ToolTip = sourceFilePath;
            }
            else
            {
                ToolTip = $"(절대 경로)\n{sourceFilePath}";
            }
        }

        private void UpdateImage(BitmapImage? image)
        {
            ImageBox.Source = image;
        }

        public void MarkAsSelected()
        {
            SelectionBorder.Visibility = System.Windows.Visibility.Visible;
        }

        public void MarkAsUnselected()
        {
            SelectionBorder.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}
