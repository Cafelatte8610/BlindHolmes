# 盲目の名探偵と「目」の助手 (Blind Detective & The "Eye" Assistant)

Unity × Google Gemini API で挑む、対話型ミステリーアドベンチャー。

## 📖 ゲーム概要

**最強の探偵は、何も見えない。**
**頼れるのは、あなたの「説明力」だけ。**

これは、決まった選択肢を選ぶだけのミステリーゲームではありません。
相棒は、**最新AIを搭載した「盲目の名探偵」**。彼は論理の天才ですが、現場の光景を一切見ることができません。

プレイヤーであるあなたは現場を歩き回り、残された証拠品を自分の目で観察します。
そして、その特徴を**「自分の言葉」**で探偵に伝えなければなりません。

* 「ナイフが落ちている」とだけ伝えるか？
* それとも、「刃こぼれしたナイフがあり、柄の部分だけが不自然に拭き取られている」と伝えるか？

あなたの説明の解像度が低ければ、探偵は推理を誤り、無実の人間を犯人だと断定してしまうかもしれません。逆に、あなたの観察眼が鋭ければ、探偵は鮮やかに真実を導き出します。

**システムが正解を用意しない、真の「対話型」推理アドベンチャー。**
あなたの語彙力と観察力で、閉ざされた闇に光を灯してください。

---

## 🎮 ゲームをプレイする (Windows Build)

以下のリンクから実行ファイル（ビルド済みゲーム）をダウンロードしてプレイできます。
解凍後、exeファイルを実行してください。

[📥 **ゲームのダウンロード (Google Drive)**](https://drive.google.com/file/d/1EAUKyMwX9on9Bkf49ZD4OoJZxMdP1JAo/view?usp=sharing)

---

## 🛠️ 開発者・Unityエディタでの実行について

このプロジェクトは Google Gemini API を使用しています。
セキュリティ保護のため、**リポジトリ内のコードには APIキーが含まれていません。**

Unityエディタ上でプロジェクトを実行、または開発を行う場合は、ご自身で APIキーを取得・設定する必要があります。

### 手順

1.  **APIキーの取得**
    [Google AI Studio](https://aistudio.google.com/) にアクセスし、Gemini API のキー（無料枠あり）を取得してください。

2.  **Unityでの設定**
    * プロジェクトを開き、APIキー入力用のスクリプト（例: `LLMManager` や `GameConfig` など、Inspector上の該当フィールド）を見つけてください。
    * 取得したAPIキーを入力してください。
    * ※APIキーは外部に漏洩しないよう、コミットには含めないでください。

---

## 🏗️ 技術スタック

* **Engine:** Unity6
* **AI Model:** Google Gemini API (Gemini 2.0Flash)
* **Assets:**
    * [Low Poly Brick Houses](https://assetstore.unity.com/packages/3d/props/exterior/low-poly-brick-houses-131899) (Environment)
    * [City-Themed Low-Poly Characters – Free Pack](https://assetstore.unity.com/packages/3d/characters/city-themed-low-poly-characters-free-pack-324242) (Charactor)
    * [Quick Outline](https://assetstore.unity.com/packages/tools/particles-effects/quick-outline-115488) (Effect)
    * その他licenseファイルに記入 

