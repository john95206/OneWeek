using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using Summer;
using System.Linq;

public class PanelTextController : MonoBehaviour {
	
	public float speed = 1.0f;
	/// <summary>
	/// TextBoxに表示するテキストをバッファしておく
	/// </summary>
	public List<string> textBuffer = new List<string>();

	List<string> defaultTextBuffer = new List<string>
	{
		"た～まや～",
		"さくしゃのTwitter → @john95206",
		"がんばって！",
		"リトライは R キー",
		"フィーバータイム後は　いがいと　チャンス",
		"なんどでもよみがえるさ",
		"て　さえ　にぎってしまえば　こっちのもんよ",
		"なつだもの",
		"花火は 300発分 だよ"
	};

	Vector2 initPos;
	float leftLimitPosX = 0;
	float RightLimitPosX = 0;

	/// <summary>
	/// チュートリアルの表示内容
	/// </summary>
	List<string> TutorialTextList = new List<string>
	{
		"花火大会が おわる までに カノジョ の 手を にぎろう！",
		"← か A キーで カノジョ に 近づこう!",
		"明るい ときに うごくと　にげられちゃうぞ！",
	};

	List<string> GameOverTextList = new List<string>
	{
		"ざんねんでした　　リトライは R キーだよ",
		"リトライは R キーです　お足もとにお気をつけてお忘れ物のないようにおかえりください",
		"R キーでリトライ　しばし花火を楽しむがいいさ",
		"R キーでリトライ　この花火でも見て　げんきだしなよ"
	};

	/// <summary>
	/// エンディング中のテキスト
	/// </summary>
	List<string> EndingTextList = new List<string>
	{
		"Congraturation!",
		"- Credit -",
		"使用アセット：UniRx　DOTween",
		"使用フォント：Pixel M Plus Regular10",
		"使用楽曲：エンディングテーマ「ピコピコ天国」from MusMus",
		"ヒューンの音 from Music is VFR",
		"パンの音 from ニコニ・コモンズ",
		"使用ツール：Edge",
		"Directed by Yuu from BIT-BAD COMPANY",
		"Thank you for playing!",
		"さくしゃのTwitter → @john95206",
	};

	/// <summary>
	/// 表示するテキスト
	/// </summary>
	[SerializeField]
	private Text TextBox;
	FireWorksManager manager;
	Tweener moveTween;

	public void Initialize(FireWorksManager manager)
	{
		initPos = transform.localPosition;
		TextBox = GetComponent<Text>();
		this.manager = manager;
		
		if (DataMaster.gameState <= GAME_STATE.Tutorial)
		{
			foreach (var text in TutorialTextList)
			{
				BufferText(text);
			}
		}
		SetTextBoxAndTween();
	}

	void SetTextBoxAndTween()
	{
		AddDefaultTextIfBufferEmpty();
		// リストの先頭を取り出す
		var nextText = textBuffer.FirstOrDefault();

		// 先頭のテキストをリストからクリア
		if (textBuffer.Count > 0)
		{
			textBuffer.RemoveAt(0);
		}

		// テキストボックスに文字をセット
		TextBox.text = nextText;
		var textStartPosX = initPos.x;
		// 文字数に応じて位置をずらす
		transform.localPosition = new Vector2(TextBox.preferredWidth, transform.localPosition.y);
		RightLimitPosX = transform.localPosition.x;
		leftLimitPosX = -RightLimitPosX;

		moveTween = transform
			.DOLocalMoveX(leftLimitPosX, TextBox.text.Length * 0.4f)
			.SetEase(Ease.Linear)
			.OnComplete(() =>
			{
				// チュートリアルの最後の文言の時はチュートリアルステート終了
				if (nextText == TutorialTextList.Last())
				{
					DataMaster.GameStart();
				}

				// 非同期でテキストは追加され続けるので、再帰処理をして常に次のテキストを流し続ける
				SetTextBoxAndTween();
			});
	}

	/// <summary>
	/// もしバッファが空ならデフォルト分から補充
	/// 表示されているものと被らないようにする
	/// </summary>
	void AddDefaultTextIfBufferEmpty()
	{
		if(textBuffer.Count < 1)
		{
			if(DataMaster.gameState == GAME_STATE.Ending)
			{
				// エンディングの全てのテキストが流れた後
				return;
			}
			var index = (int)Random.Range(0, defaultTextBuffer.Count);
			textBuffer.Add(defaultTextBuffer.Where(text => text != TextBox.text).ElementAt(index));
		}
	}

	/// <summary>
	/// 更新するテキストをバッファする
	/// </summary>
	void BufferText(string newText)
	{
		textBuffer.Add(newText);
	}

	/// <summary>
	/// FireWorksManagerから呼ぶ。
	/// 更新するテキストを即座に流す
	/// </summary>
	/// <param name="newText"></param>
	public void ForceBufferText(string newText)
	{
		textBuffer.Insert(0, newText);
		moveTween.Kill();
		SetTextBoxAndTween();
	}

	/// <summary>
	/// 強制的に次のテキストを流す
	/// チュートリアルを想定
	/// </summary>
	public void ForceUpdateText()
	{
		if(textBuffer.Count < 2)
		{
			return;
		}
		moveTween.Kill();
		SetTextBoxAndTween();
	}

	public void GameClear()
	{
		textBuffer.Clear();
		textBuffer.AddRange(EndingTextList);
		moveTween.Kill();
		SetTextBoxAndTween();
	}

	public void GameOver(bool isFireRemain)
	{
		if (isFireRemain)
		{
			var index = (int)Random.Range(0, GameOverTextList.Count);
			textBuffer.Clear();
			textBuffer.Add(GameOverTextList.ElementAt(index));
		}
		else
		{
			textBuffer.Clear();
			textBuffer.Add("これにて　しゅうりょう　またきてね　リトライは　Rだよ");
		}
		moveTween.Kill();
		SetTextBoxAndTween();
	}
}