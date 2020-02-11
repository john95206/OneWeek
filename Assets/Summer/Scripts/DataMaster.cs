using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Summer
{
	public enum GAME_STATE
	{
		Opening,
		Tutorial,
		Easy,
		Normal,
		Hard,
		Ending,
	}

	public static class DataMaster
	{
		public static GAME_STATE gameState;
		public static bool isFirstMove = true;
		/// <summary>
		/// 花火が開花した後に消えるまでの時間
		/// </summary>
		public static float fireLifeTime = 0.6f;

		/// <summary>
		/// 明るい状態かどうか
		/// </summary>
		public static bool isFlashing = false;

		/// <summary>
		/// ゲームオーバーかどうか
		/// </summary>
		public static bool isGameOver = false;

		public static bool isGameClear = false;

		/// <summary>
		/// 花火の最大数
		/// </summary>
		public static int fireWorkMaxNum = 300;

		/// <summary>
		/// データを初期化する
		/// </summary>
		public static void RefreshFlag()
		{
			isFirstMove = true;
			isFlashing = false;
			isGameOver = false;
			isGameClear = false;
			gameState = GAME_STATE.Opening;
		}

		public static void TutorialStart()
		{
			gameState = GAME_STATE.Tutorial;
		}

		public static void GameStart()
		{
			gameState = GAME_STATE.Easy;
		}

		/// <summary>
		/// ゲームオーバー
		/// </summary>
		public static void GameOver()
		{
			if(gameState < GAME_STATE.Tutorial)
			{
				return;
			}
			isGameOver = true;
		}

		/// <summary>
		/// ゲームクリア
		/// </summary>
		public static void GameClear()
		{
			isGameClear = true;
			gameState = GAME_STATE.Ending;
		}
	}
}
