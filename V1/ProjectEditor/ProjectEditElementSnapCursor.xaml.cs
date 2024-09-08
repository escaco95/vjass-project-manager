using System.Windows.Controls;

namespace vJassMainJBlueprint.V1.ProjectEditor
{
    /// <summary>
    /// ProjectEditElementSnapCursor.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ProjectEditElementSnapCursor : UserControl
    {
        public ProjectEditElementSnapCursor()
        {
            InitializeComponent();
        }

        public void SetPosition(double x, double y)
        {
            Canvas.SetLeft(this, x - 251);
            Canvas.SetTop(this, y - 251);
        }
    }
}
