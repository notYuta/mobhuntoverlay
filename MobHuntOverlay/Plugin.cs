using Dalamud.IoC;
using Dalamud.Plugin;
using System;
using System.IO;
using System.Text.Json;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using MobHuntOverlay.Services;
using MobHuntOverlay.Models;

namespace MobHuntOverlay;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;

    private MapMarkerManager MapMarkerManager { get; init; }

    public Plugin()
    {
        MapMarkerManager = new MapMarkerManager(ClientState, DataManager, Log);
        LoadMobLocationData();

        // マップが開かれるたびにマーカーを再追加
        AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "AreaMap", OnMapRefresh);
        AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, "AreaMap", OnMapRefresh);
        AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "AreaMap", OnMapRefresh);

        // Dalamud警告を抑制（このプラグインはUIウィンドウを持たない）
        PluginInterface.UiBuilder.OpenConfigUi += () => { };
        PluginInterface.UiBuilder.OpenMainUi += () => { };

        Log.Information("MobHuntOverlay loaded successfully");
    }

    private void OnMapRefresh(AddonEvent type, AddonArgs args)
    {
        MapMarkerManager.RefreshMarkers();
    }

    private void LoadMobLocationData()
    {
        try
        {
            var jsonPath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "Data", "MobLocations.json");
            if (!File.Exists(jsonPath))
            {
                Log.Error($"MobLocations.json not found at {jsonPath}");
                return;
            }

            var jsonContent = File.ReadAllText(jsonPath);
            var data = JsonSerializer.Deserialize<MobLocationData>(jsonContent);
            if (data != null)
            {
                MapMarkerManager.LoadMobLocationData(data);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load mob location data");
        }
    }

    public void Dispose()
    {
        AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, "AreaMap", OnMapRefresh);
        AddonLifecycle.UnregisterListener(AddonEvent.PostRefresh, "AreaMap", OnMapRefresh);
        AddonLifecycle.UnregisterListener(AddonEvent.PostUpdate, "AreaMap", OnMapRefresh);

        MapMarkerManager.Dispose();
    }
}
