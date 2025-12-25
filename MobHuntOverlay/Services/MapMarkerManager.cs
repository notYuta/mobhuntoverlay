using System;
using System.Numerics;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;
using MobHuntOverlay.Models;

namespace MobHuntOverlay.Services;

public unsafe class MapMarkerManager : IDisposable
{
    private readonly IClientState clientState;
    private readonly IDataManager dataManager;
    private readonly IPluginLog log;

    public MobLocationData? MobLocationData { get; private set; }

    // マーカーアイコンID
    private const uint MarkerIconId = 61710;

    public MapMarkerManager(IClientState clientState, IDataManager dataManager, IPluginLog log)
    {
        this.clientState = clientState;
        this.dataManager = dataManager;
        this.log = log;
    }

    public void LoadMobLocationData(MobLocationData data)
    {
        MobLocationData = data;
        log.Information($"Loaded mob location data version {data.Version}");
    }

    private unsafe void AddMarkersForCurrentTerritory()
    {
        var agentMap = AgentMap.Instance();
        if (agentMap == null) return;

        // マーカーリセット
        agentMap->ResetMapMarkers();

        if (MobLocationData == null) return;

        // 1. 表示すべきテリトリーデータの決定
        var territoryId = (ushort)agentMap->SelectedTerritoryId;
        if (territoryId == 0)
        {
            territoryId = clientState.TerritoryType;
        }

        var territoryData = MobLocationData.Data.Find(t => t.TerritoryTypeId == territoryId);
        if (territoryData == null) return;

        // 2. マップタイトルからターゲット名抽出
        var mapTitle = agentMap->MapTitleString.ToString();
        string? targetMobName = null;

        if (!string.IsNullOrEmpty(mapTitle))
        {
            // パターン: "リスキーモブ手配書（モブ名）" ... 全角カッコ
            var start = mapTitle.LastIndexOf('（');
            var end = mapTitle.LastIndexOf('）');
            
            if (start != -1 && end != -1 && end > start)
            {
                targetMobName = mapTitle.Substring(start + 1, end - start - 1).Trim();
            }
            else
            {
                // 英語/半角カッコ対応
                start = mapTitle.LastIndexOf('(');
                end = mapTitle.LastIndexOf(')');
                
                if (start != -1 && end != -1 && end > start)
                {
                    targetMobName = mapTitle.Substring(start + 1, end - start - 1).Trim();
                }
            }
        }

        // ターゲット名がない場合は表示しない（通常のマップ表示を妨げない）
        if (string.IsNullOrEmpty(targetMobName)) return;

        // 3. マーカー描画設定
        ushort sizeFactor = 100;
        short offsetX = 0;
        short offsetY = 0;

        var mapId = agentMap->SelectedMapId > 0 ? agentMap->SelectedMapId : clientState.MapId;

        if (dataManager.GetExcelSheet<Map>().TryGetRow(mapId, out var mapRow))
        {
            sizeFactor = mapRow.SizeFactor;
            offsetX = mapRow.OffsetX;
            offsetY = mapRow.OffsetY;
        }

        var bnpcSheet = dataManager.GetExcelSheet<BNpcName>();

        foreach (var mob in territoryData.Mobs)
        {
            bool isMatch = false;

            // 1. ローカライズ名で一致チェック
            if (bnpcSheet != null && bnpcSheet.TryGetRow(mob.BNpcNameId, out var bnpcRow))
            {
                var localName = bnpcRow.Singular.ToString();
                
                if (!string.IsNullOrEmpty(localName) && string.Equals(targetMobName, localName, StringComparison.OrdinalIgnoreCase))
                {
                    isMatch = true;
                }
                else if (!string.IsNullOrEmpty(localName) && localName.Contains(targetMobName, StringComparison.OrdinalIgnoreCase))
                {
                    isMatch = true;
                }
            }

            // 2. 英語名でもチェック
            if (!isMatch && !string.IsNullOrEmpty(mob.MobName))
            {
                if (string.Equals(targetMobName, mob.MobName, StringComparison.OrdinalIgnoreCase))
                {
                    isMatch = true;
                }
            }
            
            if (!isMatch) continue;
            
            foreach (var location in mob.Locations)
            {
                var worldPos = MapCoordToWorld(location.X, location.Y, sizeFactor, offsetX, offsetY);
                agentMap->AddMapMarker(worldPos, MarkerIconId, scale: 0);
            }
        }
    }

    /// <summary>
    /// マップ座標からワールド座標に変換
    /// </summary>
    private Vector3 MapCoordToWorld(float mapX, float mapY, ushort sizeFactor, short offsetX, short offsetY)
    {
        var scale = sizeFactor / 100.0f;
        var worldX = (mapX - 21.0f) * 50.0f / scale - offsetX;
        var worldZ = (mapY - 21.0f) * 50.0f / scale - offsetY;
        
        return new Vector3(worldX, 0, worldZ);
    }

    /// <summary>
    /// マーカーを再追加
    /// </summary>
    public unsafe void RefreshMarkers()
    {
        AddMarkersForCurrentTerritory();
    }

    public void Dispose()
    {
    }
}
