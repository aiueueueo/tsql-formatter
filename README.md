# T-SQL Formatter

SQL Server Management Studio (SSMS) 22 用の T-SQL クエリフォーマットプラグインです。

## 機能

- T-SQL クエリの自動フォーマット
- カスタマイズ可能なフォーマットルール
- プリセット機能（複数のスタイルを保存・切替）
- 選択範囲のみのフォーマット対応

## 動作環境

- SQL Server Management Studio 22
- .NET Framework 4.8

## インストール

### 自動インストール（推奨）

1. プロジェクトをビルド
   ```powershell
   dotnet build src/Plugin/TSqlFormatter.Extension.csproj -c Release
   ```

2. **管理者権限**の PowerShell でインストールスクリプトを実行
   ```powershell
   .\scripts\install-ssms.ps1
   ```

3. SSMS を再起動

### 手動インストール

1. プロジェクトをビルド
   ```powershell
   dotnet build src/Plugin/TSqlFormatter.Extension.csproj -c Release
   ```

2. 以下のファイルを SSMS の拡張機能フォルダにコピー

   **コピー元**: `src\Plugin\bin\Release\net48\`

   **コピー先**: `C:\Program Files\Microsoft SQL Server Management Studio 22\Release\Common7\IDE\Extensions\TSqlFormatter\`

   | ファイル | 説明 |
   |---------|------|
   | `TSqlFormatter.Extension.dll` | メインアセンブリ |
   | `TSqlFormatter.Core.dll` | コアライブラリ |
   | `Microsoft.SqlServer.TransactSql.ScriptDom.dll` | SQL パーサー |
   | `Newtonsoft.Json.dll` | JSON ライブラリ |

3. `src\Plugin\TSqlFormatter.Extension.pkgdef` を同じフォルダにコピー

4. 以下の内容で `extension.vsixmanifest` を作成してコピー
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <PackageManifest Version="2.0.0" xmlns="http://schemas.microsoft.com/developer/vsx-schema/2011">
     <Metadata>
       <Identity Id="TSqlFormatter.Extension.a1b2c3d4-e5f6-7890-abcd-ef1234567890" Version="1.0.0" Language="en-US" Publisher="T-SQL Formatter" />
       <DisplayName>T-SQL Formatter</DisplayName>
       <Description>A SQL Server Management Studio extension for formatting T-SQL queries.</Description>
     </Metadata>
     <Installation AllUsers="true">
       <InstallationTarget Id="Microsoft.VisualStudio.Ssms" Version="[17.0,)" />
     </Installation>
     <Dependencies>
       <Dependency Id="Microsoft.Framework.NDP" DisplayName="Microsoft .NET Framework" Version="[4.8,)" />
     </Dependencies>
     <Assets>
       <Asset Type="Microsoft.VisualStudio.VsPackage" Path="TSqlFormatter.Extension.pkgdef" />
     </Assets>
   </PackageManifest>
   ```

5. SSMS のキャッシュをクリア（推奨）
   ```powershell
   Remove-Item "$env:LocalAppData\Microsoft\SSMS\22.0*\ComponentModelCache\*" -Recurse -Force
   ```

6. SSMS を再起動

## アンインストール

**管理者権限**の PowerShell で実行：
```powershell
.\scripts\uninstall-ssms.ps1
```

または手動で以下のフォルダを削除：
```
C:\Program Files\Microsoft SQL Server Management Studio 22\Release\Common7\IDE\Extensions\TSqlFormatter
```

## 使い方

### メニューからの実行

1. SSMS でクエリを開く
2. **ツール** → **T-SQL Formatter** → **Format T-SQL**

### 選択範囲のフォーマット

1. フォーマットしたい部分を選択
2. **ツール** → **T-SQL Formatter** → **Format T-SQL**

選択範囲がない場合は、ドキュメント全体がフォーマットされます。

### 設定

**ツール** → **T-SQL Formatter** → **Settings...** で設定画面を開きます。

## フォーマットルール

### 基本ルール

| 項目 | 設定 |
|------|------|
| インデント | タブ |
| キーワード | 大文字（SELECT, FROM, WHERE 等） |
| カンマ位置 | 行頭 |
| 改行 | 各句ごと（SELECT, FROM, WHERE, JOIN 等） |
| 演算子周りのスペース | あり |
| AS キーワード | 強制（エイリアスには必ず AS を付ける） |

### フォーマット例

**Before:**
```sql
select id,name,created_at from users u inner join orders o on u.id=o.user_id where u.status='active' and o.amount>100
```

**After:**
```sql
SELECT
    u.id
    , u.name
    , u.created_at
FROM users AS u
INNER JOIN orders AS o
    ON u.id = o.user_id
WHERE u.status = 'active'
    AND o.amount > 100
```

## 設定ファイル

設定ファイルは以下の優先順位で読み込まれます：

| 優先度 | 場所 | 用途 |
|--------|------|------|
| 1（高） | プロジェクトフォルダの `.sqlformatter.json` | チーム共有用 |
| 2（低） | `%APPDATA%\T-SQL Formatter\settings.json` | 個人デフォルト |

## トラブルシューティング

### メニューが表示されない

1. SSMS を完全に終了
2. SSMS のキャッシュをクリア
   ```powershell
   Remove-Item "$env:LocalAppData\Microsoft\SSMS\22.0*\ComponentModelCache\*" -Recurse -Force
   ```
3. SSMS を再起動

### ログの確認

ログファイルは以下の場所に出力されます：
```
%APPDATA%\T-SQL Formatter\logs\
```

## 開発

### ビルド

```powershell
# 全体ビルド
dotnet build -c Release

# テスト実行
dotnet test
```

### プロジェクト構成

```
tsql-formatter/
├── src/
│   ├── Core/              # フォーマットロジック
│   │   ├── Formatter.cs
│   │   ├── Parser/        # ScriptDom ラッパー
│   │   └── Rules/         # フォーマットルール
│   └── Plugin/            # SSMS プラグイン
│       ├── Commands/      # コマンドハンドラ
│       ├── Views/         # WPF 設定画面
│       └── ViewModels/    # MVVM ViewModel
├── scripts/               # スクリプト
│   ├── install-ssms.ps1   # インストール
│   └── uninstall-ssms.ps1 # アンインストール
└── tests/                 # ユニットテスト
```

### 技術スタック

- **SQL パーサー**: Microsoft.SqlServer.TransactSql.ScriptDom
- **UI フレームワーク**: WPF
- **設定ファイル**: JSON (Newtonsoft.Json)

## ライセンス

MIT License

## 貢献

バグ報告や機能要望は Issue でお知らせください。
