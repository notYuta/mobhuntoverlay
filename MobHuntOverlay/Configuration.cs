using Dalamud.Configuration;

namespace MobHuntOverlay;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    /// <summary>
    /// マーカーアイコンID
    /// </summary>
    public uint MarkerIconId { get; set; } = 61710;

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
