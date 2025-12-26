using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace MobHuntOverlay.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    // プリセットアイコン
    private static readonly (string Name, uint Id)[] PresetIcons =
    [
        ("デフォルト（青マーカー）", 61710),
        ("赤マーカー", 60561),
        ("黄マーカー", 60492),
        ("フラッグ", 60071),
        ("カスタム", 0),
    ];

    private int selectedPreset;
    private int customIconId;

    public ConfigWindow(Configuration configuration)
        : base("MobHuntOverlay 設定###MobHuntOverlayConfig")
    {
        this.configuration = configuration;

        Flags = ImGuiWindowFlags.AlwaysAutoResize;

        // 現在の設定からプリセットを選択
        selectedPreset = Array.FindIndex(PresetIcons, p => p.Id == configuration.MarkerIconId);
        if (selectedPreset < 0)
        {
            selectedPreset = PresetIcons.Length - 1; // カスタム
        }
        customIconId = (int)configuration.MarkerIconId;
    }

    public override void Draw()
    {
        ImGui.Text("マーカーアイコン設定");
        ImGui.Separator();

        // プリセット選択
        ImGui.Text("プリセット:");
        if (ImGui.BeginCombo("##preset", PresetIcons[selectedPreset].Name))
        {
            for (var i = 0; i < PresetIcons.Length; i++)
            {
                var isSelected = selectedPreset == i;
                if (ImGui.Selectable($"{PresetIcons[i].Name} ({PresetIcons[i].Id})", isSelected))
                {
                    selectedPreset = i;
                    if (PresetIcons[i].Id != 0)
                    {
                        customIconId = (int)PresetIcons[i].Id;
                    }
                }
                if (isSelected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }

        ImGui.Spacing();

        // カスタムアイコンID入力
        ImGui.Text("アイコンID:");
        if (ImGui.InputInt("##iconId", ref customIconId))
        {
            // カスタムに切り替え
            if (customIconId > 0)
            {
                var matchingPreset = Array.FindIndex(PresetIcons, p => p.Id == (uint)customIconId);
                selectedPreset = matchingPreset >= 0 ? matchingPreset : PresetIcons.Length - 1;
            }
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        // 保存ボタン
        if (ImGui.Button("保存"))
        {
            if (customIconId > 0)
            {
                configuration.MarkerIconId = (uint)customIconId;
                configuration.Save();
            }
        }

        ImGui.SameLine();

        if (ImGui.Button("閉じる"))
        {
            IsOpen = false;
        }
    }

    public void Dispose()
    {
    }
}
