using Dalamud.IoC;
using Dalamud.Plugin;
using System;
using System.IO;
using System.Text.Json;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using MobHuntOverlay.Models;
using MobHuntOverlay.Services;
using MobHuntOverlay.Windows;

namespace MobHuntOverlay;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;

    public static Configuration Configuration { get; private set; } = null!;
    private readonly WindowSystem windowSystem = new("MobHuntOverlay");
    private readonly ConfigWindow configWindow;
    private readonly MapMarkerService mapMarkerService;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        configWindow = new ConfigWindow(Configuration);
        windowSystem.AddWindow(configWindow);

        mapMarkerService = new MapMarkerService(ClientState, DataManager, Log, Configuration);
        LoadMobLocationData();

        // マップが開かれるたびにマーカーを再追加
        AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "AreaMap", OnMapRefresh);
        AddonLifecycle.RegisterListener(AddonEvent.PostRefresh, "AreaMap", OnMapRefresh);
        AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "AreaMap", OnMapRefresh);

        // UI描画
        PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += () => configWindow.IsOpen = true;
        PluginInterface.UiBuilder.OpenMainUi += () => configWindow.IsOpen = true;

        Log.Information("MobHuntOverlay loaded successfully");
    }

    private void OnMapRefresh(AddonEvent type, AddonArgs args)
    {
        mapMarkerService.RefreshMarkers();
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
                mapMarkerService.LoadMobLocationData(data);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load mob location data");
        }
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= windowSystem.Draw;

        AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, "AreaMap", OnMapRefresh);
        AddonLifecycle.UnregisterListener(AddonEvent.PostRefresh, "AreaMap", OnMapRefresh);
        AddonLifecycle.UnregisterListener(AddonEvent.PostUpdate, "AreaMap", OnMapRefresh);

        windowSystem.RemoveAllWindows();
        configWindow.Dispose();
        mapMarkerService.Dispose();
    }
}
