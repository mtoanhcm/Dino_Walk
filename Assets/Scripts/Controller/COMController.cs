using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;

public class COMController : MonoBehaviour, IScore {

	[SerializeField] private Transform _myDeck;
	[SerializeField] private Transform _myDeckPos;
	[SerializeField] private Transform _myDinoTray;
	[SerializeField] private Transform _myDinoTrayPos;
	[SerializeField] private Transform _mainBoardDino;
	[SerializeField] private Transform[] _enemyDinoTrays;
	[SerializeField] private TextMesh _myScoreText;

	private int _myTurn;
	private bool _isDrawCard;
	private bool _isHaveMagic;
	private List<GameObject> _cardsInHand;
	private Transform _firstEnemyDinoTray, _secondEnemyDinoTray, _thirdEnemyDinoTray;
	private int _score;
	private List<Transform> _enemyWhoNeedToTakenDino;
	private List<Transform> _dinoOnBoard;
	private Transform _playerWhoWillBeTarget;
	private Transform _dinoWhoWillBeTarget;
	private GameObject _myMagicCard;

	void Awake(){
		_myTurn = int.Parse (gameObject.tag);
		_cardsInHand = new List<GameObject> ();
		_enemyWhoNeedToTakenDino = new List<Transform> ();
		_dinoOnBoard = new List<Transform> ();
		_score = 0;
		_myScoreText.GetComponent<MeshRenderer> ().sortingOrder = 6;
	}

	// Use this for initialization
	void Start () {
		StartCoroutine (COMPlay ());
	}

	IEnumerator COMPlay(){
		if (GameplayController.instance.GetIsStarPlay ()) {
			if (GameplayController.instance.GetTurn () == _myTurn) {
				yield return new WaitForSeconds (1f);
				DrawCard ();
				yield return new WaitForSeconds (2);
				DetectMagicCard ();
				if (!_isHaveMagic) {
					KnowDinoOnBoard ();
					LookThroughEnemyDinoTrays ();
					DetectLongestDinoLineInEnemyDinoTray ();
					yield return new WaitForSeconds (0.5f);
					PlayCard ();//Take 2.3s to complete this action
					yield return new WaitForSeconds (1.5f);
					TakeDinos ();
				} else {
					yield return new WaitForSeconds (1f);
					PlayMagicCard ();
					yield return new WaitForSeconds (2f);
				}
				yield return new WaitForSeconds (1f);

				EndTurn ();
			}
		}

		yield return new WaitForSeconds (0.2f);
		StartCoroutine (COMPlay ());
	}

	void EndTurn(){
		bool _isCompelete = GameplayController.instance.GetIsCompleteAction ();
		if (_isCompelete) {
			GameplayController.instance.ScaleAll ();
			_score = GameplayController.instance.CalculateMyScore (_myDinoTray, transform.GetChild (4));
			_myScoreText.text = "" + _score;
			GameplayController.instance.CheckEndGame (_myDinoTray);
			GameplayController.instance.SetIsCompleteMagic (false);
			int _nextTurn = _myTurn;
			_nextTurn++;
			if (_nextTurn > 3) {
				_nextTurn = 0;
			}
				
			GameplayController.instance.SetTurn (_nextTurn);
			_isDrawCard = false;
			_isHaveMagic = false;
			GameplayController.instance.SetIsCompleteAction (false);
		}
	}

	void SeeCardsInMyHand(){
		_cardsInHand.Clear ();
		foreach (Transform _card in _myDeck) {
			_cardsInHand.Add (_card.gameObject);
		}
	}

	void DetectMagicCard(){
		_myMagicCard = null;
		foreach (Transform _card in _myDeck) {
			if (_card.tag == "Magic") {
				_isHaveMagic = true;
				_myMagicCard = _card.gameObject;
			}
		}
	}

	void PlayCard(){
		GameplayController.instance.ShowCard (CardHaveToChoose());
		GameplayController.instance.SetIsCompleteAction (true);

		GameplayController.instance.ArrangeCardDeckOfPlayer (_myDeck, _myDeckPos);
	}

	void PlayMagicCard(){
		GameplayController.instance.ShowCard (_myMagicCard);
		GameplayController.instance.SetIsCompleteAction (true);

	}

	void DrawCard(){
		if (!_isDrawCard) {
			GameplayController.instance.SetIsCompleteDrawCard (false);
			_isDrawCard = true;
		}
	}

	void TakeDinos(){
		GameplayController.instance.TakeDinosWithCard (_myDinoTray, _myDinoTrayPos);
	}

	void LookThroughEnemyDinoTrays(){
		_firstEnemyDinoTray = _enemyDinoTrays [0];
		_secondEnemyDinoTray = _enemyDinoTrays [1];
		_thirdEnemyDinoTray = _enemyDinoTrays [2];

		DetectLongestDinoLineInEnemyDinoTray ();
	}

	void DetectLongestDinoLineInEnemyDinoTray(){
		_enemyWhoNeedToTakenDino.Clear ();

		var _dinoOfFirstEnemy = _firstEnemyDinoTray.GetComponentsInChildren<Transform> ();
		var _dinoOfSecondEnemy = _secondEnemyDinoTray.GetComponentsInChildren<Transform> ();
		var _dinoOfThirdEnemy = _thirdEnemyDinoTray.GetComponentsInChildren<Transform> ();

		DetectEnemyDinoAtFrontAndLast (_dinoOfFirstEnemy);
		DetectEnemyDinoAtFrontAndLast (_dinoOfSecondEnemy);
		DetectEnemyDinoAtFrontAndLast (_dinoOfThirdEnemy);

		ThinkAboutTargetEnemy ();
	}

	/// <summary>
	/// Detects the enemy dino at front and last.
	/// Then Add enemy who have line 2 same dino at front or last to list _enemyWhoNeedToTakenDino
	/// </summary>
	/// <param name="_dinosOfEnemy">Dinos of enemy.</param>
	void DetectEnemyDinoAtFrontAndLast(Transform[] _dinosOfEnemy){
		if (_dinosOfEnemy.Length > 2) {
			if (_dinosOfEnemy [1].name == _dinosOfEnemy [2].name) {
				_enemyWhoNeedToTakenDino.Add (_dinosOfEnemy[1].parent.parent);
			}else if (_dinosOfEnemy [_dinosOfEnemy.Length - 1].name == _dinosOfEnemy [_dinosOfEnemy.Length - 2].name) {
				if (!_enemyWhoNeedToTakenDino.Contains (_dinosOfEnemy [_dinosOfEnemy.Length - 1])) {
					_enemyWhoNeedToTakenDino.Add (_dinosOfEnemy [1].parent.parent);
				}
			}
		}
	}

	/// <summary>
	/// Think about about target enemy.
	/// with more enemy in _enemyWhoNeedToTakenDino list, choose enemy have highest scores
	/// </summary>
	void ThinkAboutTargetEnemy(){
		for (int i = 0; i < _enemyWhoNeedToTakenDino.Count - 1; i++) {
			int _firstEnemyScore = _enemyWhoNeedToTakenDino[i].GetComponent<IScore> ().GetMyScore ();
			int _nextEnemyScore = _enemyWhoNeedToTakenDino[i+1].GetComponent<IScore> ().GetMyScore ();

			if (_firstEnemyScore < _nextEnemyScore) {
				var temp = _enemyWhoNeedToTakenDino[i];
				_enemyWhoNeedToTakenDino[i] = _enemyWhoNeedToTakenDino[i+1];
				_enemyWhoNeedToTakenDino[i+1] = temp;
			}
		}

		if (_enemyWhoNeedToTakenDino.Count > 0) {
			_playerWhoWillBeTarget = _enemyWhoNeedToTakenDino [0];
		}
	}

	/// <summary>
	/// Knows the max dinos can take on dino board which I can take with my card.
	/// </summary>
	/// <returns>The max dinos can take on dino board.</returns>
	/// <param name="_headOrTail">Head or tail.</param>
	int KnowMaxDinosCanTakeOnDinoBoard(int _headOrTail){
		List<int> _cardtype = new List<int> ();
		List<int> _arrayNumDinoForTake = new List<int> ();
		int _maxDino = 0;

		foreach (GameObject _card in _cardsInHand) {
			_cardtype.Add(GameplayController.instance.GetCardType (_card));
		}

		foreach (int _card in _cardtype) {
			string _strCardType = "" + _card;
			if (int.Parse (_strCardType [1].ToString ()) == _headOrTail) {//Head
				_arrayNumDinoForTake.Add (int.Parse (_strCardType [0].ToString ()));
			} else {
				_arrayNumDinoForTake.Add (int.Parse (_strCardType [0].ToString ()));
			}
		}

		_maxDino = _arrayNumDinoForTake.Max();
		return _maxDino;
	}

	GameObject CardHaveToChoose(){
		GameObject _myCard = null;
		Transform _targetEnemyDinoTray = _playerWhoWillBeTarget;
		List<Transform> _targetDinoAtTailOnBoard = new List<Transform> ();
		List<Transform> _targetDinoAtHeadOnBoard = new List<Transform> ();

		SeeCardsInMyHand ();

		if (_mainBoardDino.childCount > 3) {

			for (int i = 0; i < KnowMaxDinosCanTakeOnDinoBoard (1); i++) {
				_targetDinoAtTailOnBoard.Add (_dinoOnBoard [i]);
			}

			for (int i = 0; i < KnowMaxDinosCanTakeOnDinoBoard (0); i++) {
				_targetDinoAtHeadOnBoard.Add (_dinoOnBoard [_dinoOnBoard.Count - 1 - i]);
			}
		

			if (_targetEnemyDinoTray != null && _targetEnemyDinoTray.childCount > 2) {
				if (_targetEnemyDinoTray.GetChild (0).name == _targetEnemyDinoTray.GetChild (1).name) {
					if (_targetDinoAtTailOnBoard.Contains (_targetEnemyDinoTray.GetChild (0))) {
						_myCard = _targetEnemyDinoTray.GetChild (0).gameObject;
					}
				} else if (_targetEnemyDinoTray.GetChild (_targetEnemyDinoTray.childCount - 1).name == _targetEnemyDinoTray.GetChild (_targetEnemyDinoTray.childCount - 2).name) {
					if (_targetDinoAtHeadOnBoard.Contains (_targetEnemyDinoTray.GetChild (_targetEnemyDinoTray.childCount - 1))) {
						_myCard = _targetEnemyDinoTray.GetChild (_targetEnemyDinoTray.childCount - 1).gameObject;
					}
				}
			}
		}

		//Get card can give highest score
		if (_myCard == null) {
			for (int i = 0; i < 2; i++) {
				if(GameplayController.instance.GetCardType(_cardsInHand[i]) < GameplayController.instance.GetCardType(_cardsInHand[i+1])){
					var temp = _cardsInHand[i];
					_cardsInHand[i] = _cardsInHand[i+1];
					_cardsInHand[i+1] = temp;
				}
			}

			_myCard = _cardsInHand [0];
		}

		return _myCard;
	}

	void KnowDinoOnBoard(){
		_dinoOnBoard.Clear ();
		foreach (Transform _dino in _mainBoardDino) {
			_dinoOnBoard.Add (_dino);
		}
	}

	public int GetMyScore(){
		return _score;
	}

	public void SetMyScore(int _value){
		_score = _value;
	}

	public void SetMyScoreText(int _value){
		_myScoreText.text = "" + _value;
	}

	public Transform[] GetEnemyDinoTray(){
		return _enemyDinoTrays;
	}
}
