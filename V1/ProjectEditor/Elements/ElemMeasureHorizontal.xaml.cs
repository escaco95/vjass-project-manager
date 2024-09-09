using System.Windows;
using System.Windows.Controls;

namespace vJassMainJBlueprint.V1.ProjectEditor.Elements
{
    /// <summary>
    /// ElemMeasureHorizontal.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ElemMeasureHorizontal : UserControl
    {
        public ElemMeasureHorizontal()
        {
            InitializeComponent();
        }

        // DependencyProperty for Text
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ElemMeasureHorizontal), new PropertyMetadata("2000"));

        // CLR wrapper for Text
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
    }
}
