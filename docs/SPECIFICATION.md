# T-SQL Formatter 設計仕様書

SSMS 22用のT-SQLフォーマッタープラグイン

## プロジェクト概要

| 項目 | 内容 |
|------|------|
| プラグイン名 | T-SQL Formatter |
| 対象SSMS | 22 |
| 開発言語 | C# |
| UI | WPF |
| ライセンス | MIT |
| 配布 | 社内配布 → GitHub Releases |

## 技術スタック

- **SQLパーサー**: Microsoft.SqlServer.TransactSql.ScriptDom（NuGet: Microsoft.SqlServer.DacFx）
- **UI フレームワーク**: WPF
- **設定ファイル形式**: JSON

## フォーマットルール

### 基本ルール

| 項目 | 設定 |
|------|------|
| インデント | タブ |
| キーワード | 大文字（SELECT, FROM, WHERE等） |
| カンマ位置 | 行頭 |
| 改行 | 各句ごと（SELECT, FROM, WHERE, JOIN, ORDER BY等） |
| 演算子周りのスペース | あり（`=`, `<>`, `AND`, `OR`等） |
| 括弧内スペース | なし `ISNULL(column, 0)` |
| ASキーワード | 強制（エイリアスには必ずASを付ける） |
| 空白行 | 句の間には入れない |

### JOIN句

- 独立した行に配置
- ON句はインデントする

### サブクエリ

- サブクエリ内はインデントを深くする

### ORDER BY句

- 独立した行に配置
- ASC/DESCは省略可能（ASCはデフォルト）

### CASE文

```sql
CASE
    WHEN condition1 THEN result1
    WHEN condition2 THEN result2
    ELSE default_result
END
```

### コメント

- 既存コメントの位置を保持
- コメント内のフォーマットはそのまま維持

### エラーハンドリング

- 構文エラーのあるSQLはフォーマットしない

## フォーマット例

### Before

```sql
select id,name,created_at from users u inner join orders o on u.id=o.user_id where u.status='active' and o.amount>100
```

### After

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

## 機能仕様

### ショートカットキー

- デフォルト: `Ctrl+K`（SSMSと競合するため要変更）
- ユーザーが設定画面からカスタマイズ可能

### 適用範囲

- デフォルト: ドキュメント全体
- 設定で選択範囲のみに変更可能

### 設定画面

- WPFで実装
- すべてのフォーマットルールをカスタマイズ可能

### プリセット機能

- 複数のフォーマットスタイルを保存・切替可能

### 設定ファイルの保存場所（ハイブリッド方式）

| 優先度 | 場所 | 用途 |
|--------|------|------|
| 1（高） | プロジェクトフォルダの `.sqlformatter.json` | チーム共有用 |
| 2（低） | `%APPDATA%\T-SQL Formatter\settings.json` | 個人デフォルト |

プロジェクト設定があればそちらを優先、なければ個人設定を使用する。

## 開発フェーズ

### Phase 1（初期リリース）

- フォーマット機能
- 設定画面
- プリセット機能
- ショートカットキー対応

### Phase 2（将来の拡張）

- Linter機能
  - テーブル修飾子の自動付与（`id` → `u.id`）
  - SSMSの接続情報を利用してスキーマ取得
- その他の静的解析機能（未定）

## 対応するSQL文

- SELECT
- INSERT
- UPDATE
- DELETE
- CREATE / ALTER TABLE
- ストアドプロシージャ
- その他すべてのT-SQL構文

## アーキテクチャ

```text
T-SQL Formatter/
├── src/
│   ├── Core/              # フォーマットロジック
│   │   ├── Formatter.cs
│   │   ├── Rules/         # 各フォーマットルール
│   │   └── Parser/        # ScriptDomラッパー
│   ├── UI/                # WPF設定画面
│   │   ├── SettingsWindow.xaml
│   │   └── ViewModels/
│   ├── Settings/          # 設定管理
│   │   ├── SettingsManager.cs
│   │   └── Presets/
│   └── Plugin/            # SSMSプラグイン統合
│       └── TSqlFormatterPackage.cs
├── tests/                 # ユニットテスト
├── scripts/               # インストールスクリプト
└── docs/                  # ドキュメント
```

## テスト

- 各フォーマットルールに対するユニットテストを作成
- 複雑なSQLパターンに対するテストケースを網羅
