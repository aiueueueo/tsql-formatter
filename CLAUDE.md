# T-SQL Formatter

SSMS 22用のT-SQLフォーマッタープラグイン

## コマンド

```bash
# ビルド
dotnet build -c Release

# テスト
dotnet test

# SSMSにインストール（管理者権限必要）
powershell -ExecutionPolicy Bypass -File scripts/install-ssms.ps1
```

## コーディング規約

- C#コーディング規約に従う
- 応答は日本語

## 重要ファイル

| ファイル | 説明 |
|---------|------|
| `src/Core/Formatter.cs` | フォーマッター本体 |
| `src/Core/FormatterSettings.cs` | フォーマット設定 |
| `src/Core/Rules/FormattingVisitor.cs` | フォーマットルール実装 |
| `src/Plugin/TSqlFormatterPackage.cs` | SSMSプラグイン |
| `docs/TODO.md` | 開発タスク一覧 |
| `docs/SPECIFICATION.md` | 詳細設計仕様書 |
