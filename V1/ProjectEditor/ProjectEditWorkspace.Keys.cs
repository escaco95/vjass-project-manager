using System.Windows.Input;

namespace vJassMainJBlueprint.V1.ProjectEditor
{
    partial class ProjectEditWorkspace
    {
        public class KeyBindingSettings
        {
            public required string Key { get; set; }
            public ModifierKeys Modifiers { get; set; }
        }

        // 키 설정을 저장할 딕셔너리
        private readonly Dictionary<string, KeyBindingSettings> keyBindings = new()
        {
            { "file.new", new KeyBindingSettings(){ Key="N", Modifiers=ModifierKeys.Control } },
            { "file.open", new KeyBindingSettings(){ Key="O", Modifiers=ModifierKeys.Control } },
            { "file.close", new KeyBindingSettings() { Key="W", Modifiers=ModifierKeys.Control } },
            { "file.save", new KeyBindingSettings(){ Key="S", Modifiers=ModifierKeys.Control } },
            { "file.saveas", new KeyBindingSettings(){ Key="S", Modifiers=ModifierKeys.Control | ModifierKeys.Shift } },
            { "edit.delete", new KeyBindingSettings(){ Key="Delete", Modifiers=ModifierKeys.None } },
            { "edit.selectall", new KeyBindingSettings(){ Key="A", Modifiers=ModifierKeys.Control } },
            { "edit.unselect", new KeyBindingSettings(){ Key="Escape", Modifiers=ModifierKeys.None } },
            { "edit.copy.nodeimage", new KeyBindingSettings(){ Key="C", Modifiers=ModifierKeys.Control | ModifierKeys.Shift} },
            { "edit.paster.nodeimage", new KeyBindingSettings(){ Key="V", Modifiers=ModifierKeys.Control | ModifierKeys.Shift} },
            { "view.tool.sampleicon",  new KeyBindingSettings(){ Key = "I", Modifiers = ModifierKeys.Control | ModifierKeys.Alt } },
            { "view.allnodes",  new KeyBindingSettings(){ Key = "Space", Modifiers = ModifierKeys.None } },
        };

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // 현재 키와 수정자 확인
            var modifiers = Keyboard.Modifiers;
            var key = e.Key;

            foreach (var binding in keyBindings)
            {
                var bindingKey = (Key)Enum.Parse(typeof(Key), binding.Value.Key);
                if (bindingKey == key && binding.Value.Modifiers == modifiers)
                {
                    // 매핑된 명령 실행
                    ExecuteCommand(binding.Key);
                    e.Handled = true; // 이벤트 처리 완료
                    return;
                }
            }

            // 기본 처리 호출
            base.OnKeyDown(e);
        }

        private void ExecuteCommand(string command)
        {
            switch (command)
            {
                case "file.new":
                    OnMenuFileNewClick(null, null);
                    break;
                case "file.open":
                    OnMenuFileOpenClick(null, null);
                    break;
                case "file.close":
                    OnMenuFileCloseClick(null, null);
                    break;
                case "file.save":
                    OnMenuFileSaveClick(null, null);
                    break;
                case "file.saveas":
                    OnMenuFileSaveAsClick(null, null);
                    break;
                case "edit.delete":
                    OnMenuEditDelete(null, null);
                    break;
                case "edit.selectall":
                    OnMenuEditSelectAll(null, null);
                    break;
                case "edit.unselect":
                    OnMenuEditUnselectAll(null, null);
                    break;
                case "edit.copy.nodeimage":
                    OnMenuEditCopyImage(null, null);
                    break;
                case "edit.paster.nodeimage":
                    OnMenuEditPasteImage(null, null);
                    break;
                case "view.tool.sampleicon":
                    OnMenuViewToolSampleIcon(null, null);
                    break;
                case "view.allnodes":
                    OnMenuViewAllNodesClick(null, null);
                    break;
            }
        }
    }
}
