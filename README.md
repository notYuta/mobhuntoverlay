# MobHuntOverlay

FF14のBランクモブ手配書の「生息場所」ボタンを押すと、マップ上にスポーン位置をマーカー表示するDalamudプラグインです。

## 機能

- 手配書の「生息場所」ボタンからマップを開くと、そのBモブのスポーン位置が自動表示
- 全拡張対応（ARR, HW, SB, ShB, EW, DT）
- 通常のマップ表示を妨げない（手配書経由のみ動作）

## インストール方法

### 手動インストール

1. [Releases](../../releases)から最新の`MobHuntOverlay.zip`をダウンロード
2. 解凍した中身を以下のフォルダに配置：
   ```
   %APPDATA%\XIVLauncher\installedPlugins\MobHuntOverlay\
   ```
3. FF14を再起動

### カスタムリポジトリ経由

1. Dalamudの設定 → 試験的機能 → カスタムプラグインリポジトリ
2. 以下のURLを追加：
   ```
   https://raw.githubusercontent.com/notYuta/mobhuntoverlay/main/repo.json
   ```
3. プラグイン一覧から「MobHuntOverlay」をインストール

## 使い方

1. ゲーム内で手配書（Bランク）を開く
2. 「生息場所」ボタンをクリック
3. マップ上にスポーン位置がマーカーで表示される

## ライセンス

AGPL-3.0-or-later

## クレジット

- スポーンデータ: [ffxiv-huntmaps-maker](https://github.com/RKI027/ffxiv-huntmaps-maker)
