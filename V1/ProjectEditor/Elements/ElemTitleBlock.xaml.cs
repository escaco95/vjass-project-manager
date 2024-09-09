using System.Windows;
using System.Windows.Controls;

namespace vJassMainJBlueprint.V1.ProjectEditor.Elements
{
    /// <summary>
    /// ElemTitleBlock.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ElemTitleBlock : UserControl
    {
        public ElemTitleBlock()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ProjectNameProperty = DependencyProperty.Register("ProjectName", typeof(string), typeof(ElemTitleBlock), new PropertyMetadata("이름 없는 프로젝트"));
        public static readonly DependencyProperty ProjectAuthorProperty = DependencyProperty.Register("ProjectAuthor", typeof(string), typeof(ElemTitleBlock), new PropertyMetadata("알 수 없음"));

        public string ProjectName { get { return (string)GetValue(ProjectNameProperty); } set { SetValue(ProjectNameProperty, value); } }
        public string ProjectAuthor { get { return (string)GetValue(ProjectAuthorProperty); } set { SetValue(ProjectAuthorProperty, value); } }
    }
}
