using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameplayController : MonoBehaviour {
	
	public static GameplayController instance;

	[SerializeField] private Transform _cardGraveyard;
	[SerializeField] private GameObject _highlightEffect;
	[SerializeField] private GameObject _endPannel;
	[SerializeField] private GameObject[] _listPlayers;
	[SerializeField] private GameObject[] _eggPlayerTray;
	[SerializeField] private GameObject[] _magicCardEffect;
	[SerializeField] private GameObject _egg;
	[SerializeField] private GameObject _choosenRect;
	[SerializeField] private Transform _choosenRectList;
	[SerializeField] private GameObject _burnEffect;
	[SerializeField] private Text _timeText;
	public AudioSource _audioSource;
	[SerializeField] private AudioClip _audioClipOneDino;
	[SerializeField] private AudioClip _audioClipTwoDino;
	[SerializeField] private AudioClip _audioClipThreeDino;
	[SerializeField] private AudioClip _audioClipHybirdDino;
	[SerializeField] private AudioClip _audioClipCard;
	[SerializeField] private AudioClip _audioRopeMagic;
	[SerializeField] private AudioClip _audioEggMagic;
	[SerializeField] private AudioClip _audioVolcanoMagic;

	private int _turn;
	private int _cardType;
	private int _chooseDinoAmount;
	private bool _isStarPlay;
	private bool _isCompleteAction;//Use for Player end turn when click on Card
	private bool _isCompleteDrawCard;
	private bool _isPlayerChoosenDino;
	private bool _isCompleteMagic;
	private bool _isTimerCountdown;
	private bool _isStarGiveCard;//Use at start to make sure all dino in board before card ready to give for players
	private List<GameObject> _listDinoPlayable;
	private List<GameObject> _listHightLightEffect;

	void MakeInstance(){
		if (instance == null) {
			instance = this;
		}
	}

	// Use this for initialization
	void Awake () {
		MakeInstance ();
		_turn = 0;
		_cardType = 0;
		_isCompleteDrawCard = true;
		_listHightLightEffect = new List<GameObject> ();
	}

	public int GetTurn(){
		return _turn;
	}

	public void SetTurn(int _value){
		if (_value > 0 && _value <= 3) {
			_turn = _value;
		} else {
			_turn = 0;
		}
	}

	public void TimeCountDown(Transform _myPlayer){
		StartCoroutine (SetupTimeCountdown (_myPlayer));
	}

	/// <summary>
	/// Starts the timer count dount.
	/// </summary>
	/// <param name="_myPlayer">My player.</param>
	IEnumerator SetupTimeCountdown(Transform _myPlayer){
		yield return new WaitForSeconds (3f);
		int _time = 30;
		_timeText.gameObject.SetActive (true);
		_isTimerCountdown = true;
		StartCoroutine (TimeCountDown (_time,_myPlayer));
	}

	IEnumerator TimeCountDown(int _time, Transform _myPlayer){
		if (!_isTimerCountdown) {
			_timeText.gameObject.SetActive (false);
			yield break;
		}


		_timeText.text = "" + _time;
		_time--;
		yield return new WaitForSeconds (1f);
		if (_time <= 0) {
			ShowCard (_myPlayer.GetChild (1).GetChild (2).gameObject);
			_isPlayerChoosenDino = true;
			_timeText.gameObject.SetActive (false);
			_isTimerCountdown = false;
			yield break;
		}

		StartCoroutine (TimeCountDown(_time,_myPlayer));
	}

	/// <summary>
	/// Shows the card from COMs/Player's Deck.
	/// </summary>
	/// <param name="_card">Card.</param>
	public void ShowCard(GameObject _card){
		_audioSource.PlayOneShot (_audioClipCard);
		_isTimerCountdown = false;
		Transform _playerWhoShowThisCard = _card.transform.parent.parent;

		foreach (Transform _theCard in _playerWhoShowThisCard.GetChild(1)) {
			_theCard.GetComponent<BoxCollider2D> ().enabled = false;
		}

		_card.transform.DOMove (Vector3.zero, 0.8f).OnComplete(()=>EffectBeforeSendCardToGraveyard(_card, _playerWhoShowThisCard));
		_card.transform.DORotate (new Vector3 (0, 180, 0), 0.8f);
		_card.transform.DOScale (1, 0.8f);
		_card.transform.SetParent (null);

		if (_card.tag != "Magic") {
			_cardType = GetCardType (_card);

			StartCoroutine (GiveCardToGraveyard (_card));
		}
	}

	void EffectBeforeSendCardToGraveyard(GameObject _card, Transform _playerShow){

		if (_card.tag == "Magic") {
			if (_card.name == "Card Egg(Clone)") {
				GameObject _tempEffect = Instantiate (_magicCardEffect [2], _card.transform.position, Quaternion.identity) as GameObject;
				Destroy (_tempEffect, 0.5f);

				_audioSource.PlayOneShot (_audioEggMagic);
				EggMagicCardAction ();
			} else if (_card.name == "Card Rope(Clone)") {
				GameObject _tempEffect = Instantiate (_magicCardEffect [1], _card.transform.position, Quaternion.identity) as GameObject;
				Destroy (_tempEffect, 0.5f);

				_audioSource.PlayOneShot (_audioRopeMagic);
				RopeMagicCardAction (_playerShow);
			} else if (_card.name == "Card Volcano(Clone)") {
				GameObject _tempEffect = Instantiate (_magicCardEffect [0], _card.transform.position, Quaternion.identity) as GameObject;
				Destroy (_tempEffect, 0.5f);

				_audioSource.PlayOneShot (_audioVolcanoMagic);
				VolcanoMagicCardAction ();
			}
				
			StartCoroutine (GiveCardToGraveyard (_card));
		} else {
			PlayAudioDinoRoar (_card);
		}
	}

	void PlayAudioDinoRoar(GameObject _card){
		if (_card.tag == "1H" || _card.tag == "1T") {
			_audioSource.PlayOneShot (_audioClipOneDino);
		} else if (_card.tag == "2H" || _card.tag == "2T") {
			_audioSource.PlayOneShot (_audioClipTwoDino);
		} else if (_card.tag == "3H" || _card.tag == "3T") {
			_audioSource.PlayOneShot (_audioClipThreeDino);
		} else if (_card.tag == "HT") {
			_audioSource.PlayOneShot (_audioClipHybirdDino);
		}
	}

	/// <summary>
	/// Gives the card to graveyard.
	/// </summary>
	/// <returns>The card to graveyard.</returns>
	/// <param name="_card">Card.</param>
	IEnumerator GiveCardToGraveyard(GameObject _card){
		yield return new WaitForSeconds (2.5f);
		_card.GetComponent<BoxCollider2D> ().enabled = false;
		Vector3 _temPos = Vector3.zero;
		int _cardLayer = 0;
		if (_cardGraveyard.childCount > 0) {
			_temPos = _cardGraveyard.GetChild (_cardGraveyard.childCount - 1).position;
			_cardLayer = _cardGraveyard.GetChild (_cardGraveyard.childCount - 1).GetChild(0).GetComponent<SpriteRenderer> ().sortingOrder;
			_cardLayer++;
			_card.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = _cardLayer;
		} else {
			_temPos = _cardGraveyard.position;
			_temPos.y += 0.6f;
		}

		_temPos.x += 0.03f;
		_card.transform.SetParent (_cardGraveyard);
		_card.transform.DOMove (_temPos, 0.5f);

		ScaleAll ();
	}

	/// <summary>
	/// Eggs the magic card action.
	/// </summary>
	void EggMagicCardAction(){
		int _numOfEggToAdd = 1;
		List<int> _playerDinosNum = new List<int> ();
		foreach (GameObject _player in _listPlayers) {
			_playerDinosNum.Add (_player.transform.GetChild (3).childCount);
		}

		_playerDinosNum.Sort ();

		foreach (GameObject _player in _listPlayers) {
			var _temp = _player.transform.GetChild (3).childCount;
			int _eggCount = 0;

			foreach (Transform _myEgg in _player.transform.GetChild(4)) {
				if (_myEgg.tag == "Egg") {
					_eggCount++;
				}
			}

			if (_temp == _playerDinosNum [0] && _numOfEggToAdd > 0) {
				GameObject _myEgg = Instantiate (_egg, _player.transform.GetChild (4).GetChild(_eggCount).position, Quaternion.identity) as GameObject;
				_myEgg.transform.SetParent (_player.transform.GetChild (4));

				if (_player.tag == "1") {
					_myEgg.transform.DORotate (new Vector3 (0, 0, 90), 0.3f);
				} else if (_player.tag == "2") {
					_myEgg.transform.DORotate (new Vector3 (0, 0, 180), 0.3f);
				} else if (_player.tag == "3") {
					_myEgg.transform.DORotate (new Vector3 (0, 0, -90), 0.3f);
				}

				_numOfEggToAdd--;
			}
		}
			
		_isCompleteMagic = true;//Send Card to graveyard

	}

	/// <summary>
	/// Volcanos the magic card action.
	/// </summary>
	void VolcanoMagicCardAction(){
		var _playerDinoTrays = new List<Transform> ();
		var _dinosHaveToDie = new List<Transform> ();
		float _timeDestroy = 1.5f;

		foreach (GameObject _player in _listPlayers) {
			_playerDinoTrays.Add (_player.transform.GetChild (3));
		}

		//Get longest Dino line in tray
		foreach(Transform _tray in _playerDinoTrays){
			_dinosHaveToDie.AddRange (CheckLongestDinoLine (_tray));
		}

		//Destroy Effect
		foreach (Transform _dino in _dinosHaveToDie) {
			GameObject _effect = _dino.GetChild(0).gameObject;
			_effect.SetActive (true);
			_effect.transform.parent = null;

			_dino.DOScale (Vector3.zero, _timeDestroy - 0.5f);
			Destroy (_dino.gameObject, _timeDestroy);
			Destroy (_effect, _timeDestroy);

		}

		_isCompleteMagic = true;
	}

	/// <summary>
	/// Checks the longest dino line.
	/// </summary>
	/// <returns>The longest dino line.</returns>
	/// <param name="_tray">Tray.</param>
	List<Transform> CheckLongestDinoLine(Transform _tray){
		var _longestLine = new List<Transform> ();
		Transform _dinoNeedToDestroy = null;

		int _num = CheckDinoBothTray (_tray)[0];
		int _pos = CheckDinoBothTray (_tray)[1];


		for (int i = 0; i < _num; i++) {
			if (_pos == 0) {
				_dinoNeedToDestroy = _tray.GetChild (i);
				_longestLine.Add (_dinoNeedToDestroy);
			} else {
				if (_tray.childCount > 0) {
					_dinoNeedToDestroy = _tray.GetChild (_tray.childCount - 1 - i);
					_longestLine.Add (_dinoNeedToDestroy);
				}
			}
		}

		return _longestLine;
	}

	/// <summary>
	/// Checks the dino both tray. return array with number of dino need to destroy at [0] and position of it at [1]
	/// Use for Volcano Card
	/// </summary>
	/// <returns>The dino both tray.</returns>
	/// <param name="_tray">Tray.</param>
	int[] CheckDinoBothTray(Transform _tray){
		int[] _numDinoDestroy = new int[2];
		int _numDinoFirst = 1;
		int _numDinoLast = 1;
		int _maxDino = _tray.childCount;

		if (_maxDino > 1) {
			if (_tray.GetChild (0).name == _tray.GetChild (1).name) {
				_numDinoFirst++;
				if (_maxDino > 2) {
					if (_tray.GetChild (0).name == _tray.GetChild (2).name) {
						_numDinoFirst++;
						if (_maxDino > 3) {
							if (_tray.GetChild (0).name == _tray.GetChild (3).name) {
								_numDinoFirst++;
							}
						}
					}
				}
			}
		}

		if (_maxDino > 1) {
			if (_tray.GetChild (_maxDino-1).name == _tray.GetChild (_maxDino-2).name) {
				_numDinoLast++;
				if (_maxDino > 2) {
					if (_tray.GetChild (_maxDino-1).name == _tray.GetChild (_maxDino-3).name) {
						_numDinoLast++;
						if (_maxDino > 3) {
							if (_tray.GetChild (_maxDino-1).name == _tray.GetChild (_maxDino-4).name) {
								_numDinoLast++;
							}
						}
					}
				}
			}
		}

		if (_numDinoFirst > _numDinoLast) {
			_numDinoDestroy [0] = _numDinoFirst;
			_numDinoDestroy [1] = 0;
		} else if (_numDinoFirst < _numDinoLast) {
			_numDinoDestroy [0] = _numDinoLast;
			_numDinoDestroy [1] = 1;
		} else if(_numDinoFirst == _numDinoLast){
			if (_numDinoFirst < 2) {
				if (_tray.childCount > 0) {
					_numDinoDestroy [0] = 1;
				} else {
					_numDinoDestroy [0] = 0;
				}
			} else {
				_numDinoDestroy [0] = _numDinoFirst;
			}

			_numDinoDestroy [1] = Random.Range(0,2);
		}

		return _numDinoDestroy;
	}

	/// <summary>
	/// Ropes the magic card action.
	/// </summary>
	/// <param name="_playerShowCard">Player show card.</param>
	void RopeMagicCardAction(Transform _playerShowCard){
		_chooseDinoAmount = 1;
		List<Transform> _enemyDinoTrays = new List<Transform> ();
		List<Transform> _listEnemyDinosAtFrontAndBack = new List<Transform> ();
		Transform _myFirstDino = null;
		Transform _myLastDino = null;
		Transform _enemyHaveMostDinoNumInTray = null;

		if (_playerShowCard.GetChild (3).childCount > 0) {
			_myFirstDino = _playerShowCard.GetChild (3).GetChild (0);
			_myLastDino = _playerShowCard.GetChild (3).GetChild (_playerShowCard.GetChild (3).childCount - 1);
		}

		if (_playerShowCard.tag != "Player") {
			_enemyDinoTrays.AddRange (_playerShowCard.GetComponent<COMController> ().GetEnemyDinoTray ());

			//_enemyDinoTrays = _enemyDinoTrays.OrderBy (_enemyDinoTray => _enemyDinoTray.childCount).ToList();
			_enemyHaveMostDinoNumInTray = EnemyWhoHaveMostDino(_enemyDinoTrays);

			foreach (Transform _enemyDinoTray in _enemyDinoTrays) {
				if (_enemyDinoTray.childCount > 0) {
					_listEnemyDinosAtFrontAndBack.Add (_enemyDinoTray.GetChild (0));
					_listEnemyDinosAtFrontAndBack.Add (_enemyDinoTray.GetChild (_enemyDinoTray.childCount - 1));
				}
			}

			for (int i=0;i< _listEnemyDinosAtFrontAndBack.Count;i++) {
				var _dino = _listEnemyDinosAtFrontAndBack [i];
				if (_chooseDinoAmount > 0) {
					if (_myFirstDino != null && _myLastDino != null) {
						if (_dino.name == _myFirstDino.name) {
							_dino.SetParent (_playerShowCard.GetChild (3));
							_dino.SetAsFirstSibling ();
							_chooseDinoAmount--;
						} else if (_dino.name == _myLastDino.name) {
							_dino.SetParent (_playerShowCard.GetChild (3));
							_dino.SetAsLastSibling ();
							_chooseDinoAmount--;
						} else if (_dino.parent.parent.name == _enemyHaveMostDinoNumInTray.name) {Debug.Log (_enemyHaveMostDinoNumInTray.name);
							_dino.SetParent (_playerShowCard.GetChild (3));
							_dino.SetAsFirstSibling ();
							_chooseDinoAmount--;
						}
					} else {Debug.Log (_enemyHaveMostDinoNumInTray.name);
						if (_dino.parent.parent.name ==_enemyHaveMostDinoNumInTray.name) {
							_dino.SetParent (_playerShowCard.GetChild (3));
							_dino.SetAsFirstSibling ();
							_chooseDinoAmount--;
						}
					}
				} else {
					break;
				}
			}

		} else if (_playerShowCard.tag == "Player"){//Highlight dinos for choose
			_enemyDinoTrays.AddRange (_playerShowCard.GetComponent<PlayerController> ().GetEnemyDinoTray ());

			foreach (Transform _enemyDinoTray in _enemyDinoTrays) {
				if (_enemyDinoTray.childCount > 0) {
					_listEnemyDinosAtFrontAndBack.Add (_enemyDinoTray.GetChild (0));
					_listEnemyDinosAtFrontAndBack.Add (_enemyDinoTray.GetChild (_enemyDinoTray.childCount - 1));
				}
			}

			foreach (Transform _dino in _listEnemyDinosAtFrontAndBack) {
				if (_chooseDinoAmount > 0) {
					GameObject _choosenHighlight = Instantiate (_choosenRect, _dino.position, Quaternion.identity) as GameObject;
					_choosenHighlight.transform.SetParent (_choosenRectList);
					_dino.GetComponent<CircleCollider2D> ().enabled = true;
				}
			}

			if (_listEnemyDinosAtFrontAndBack.Count > 0) {
				_playerShowCard.GetComponent<PlayerController> ().EnablePosToPutDino ();
			} else {
				_chooseDinoAmount = 0;
				_isCompleteMagic = true;
			}

		}

		//Scale dinos in all Player's Dino Tray
		//ScaleAll();

		if (_playerShowCard.tag != "Player") {
			_isCompleteMagic = true;
		}
	}

	Transform EnemyWhoHaveMostDino(List<Transform> _enemyDinoTrays){
		for (int i = 0; i < _enemyDinoTrays.Count-1; i++) {
			for (int j = i + 1; j < _enemyDinoTrays.Count; j++) {
				if (_enemyDinoTrays [i].childCount < _enemyDinoTrays [j].childCount) {
					var _temp = _enemyDinoTrays [i];
					_enemyDinoTrays [i] = _enemyDinoTrays [j];
					_enemyDinoTrays [j] = _temp;
				}
			}
		}

		return _enemyDinoTrays [0].parent;
	}

	/// <summary>
	/// Scales all dino in tray.
	/// </summary>
	public void ScaleAll(){
		UpdateScore ();
		foreach (GameObject _player in _listPlayers) {
			for (int i = 0; i < _player.transform.GetChild (3).childCount; i++) {
				var _myDino = _player.transform.GetChild (3).GetChild (i);
				ReScaleAllDinoInTray (_myDino.gameObject, _player.transform.GetChild (2).GetChild(i));
				ArrangeDinoTray (_player.transform);
			}
		}
	}

	public void ToggleCollider(bool _value){
		foreach (GameObject _player in _listPlayers) {
			for (int i = 0; i < _player.transform.GetChild (3).childCount; i++) {
				var _myDino = _player.transform.GetChild (3).GetChild (i);
				if (_myDino.tag == "Dino") {
					_myDino.GetComponent<CircleCollider2D> ().enabled = false;
				}
			}
		}
	}

	void ReScaleAllDinoInTray(GameObject _dino, Transform _dinoTrayPos){
		//_dino.transform.DOScale (Vector3.one * 0.45f, 0.3f);
		var _player = _dino.transform.parent.parent;
		if (_dinoTrayPos.tag == "Right face") {
			_dino.GetComponent<SpriteRenderer> ().flipX = true;
		} else {
			_dino.GetComponent<SpriteRenderer> ().flipX = false;
		}


		if (_player.tag == "Player") {
			_dino.transform.DORotate (new Vector3 (0, 0, 0), 0.3f);	
		} else if (_player.tag == "1") {
			_dino.transform.DORotate (new Vector3 (0, 0, 90), 0.3f);	
		} else if (_player.tag == "2") {
			_dino.transform.DORotate (new Vector3 (0, 0, 180), 0.3f);
		} else if (_player.tag == "3") {
			_dino.transform.DORotate (new Vector3 (0, 0, -90), 0.3f);
		}
	}

	void ArrangeDinoTray(Transform _myPlayer){
		var _dinoTray = _myPlayer.GetChild (3);
		var _dinoTrayPos = _myPlayer.GetChild (2);

		for (int i = 0; i < _dinoTray.childCount; i++) {
			_dinoTray.GetChild(i).DOMove (_dinoTrayPos.GetChild (i).position, 0.5f);
		}
	}

	/// <summary>
	/// Calculates my score.
	/// </summary>
	/// <param name="_myDinoTray">My dino tray.</param>
	/// <param name="_myEggTray">My egg tray.</param>
	/// <param name="_myScore">My score.</param>
	public int CalculateMyScore(Transform _myDinoTray, Transform _myEggTray){
		var _tempDinoTray = _myDinoTray.GetComponentsInChildren<Transform> ();
		var _listMyDinoTray = new List<Transform> ();
		int _myScore = 0;
		int _eggPoint = 0;
		int _dinoNum = 0;

		_listMyDinoTray.AddRange (_tempDinoTray);
		_listMyDinoTray.RemoveAt (0);

		for (int i = 0; i < _listMyDinoTray.Count - 3; i++) {
			if (_listMyDinoTray [i].name == _listMyDinoTray [i+1].name && _listMyDinoTray[i].name == _listMyDinoTray[i+2].name && _listMyDinoTray[i].name == _listMyDinoTray[i+3].name) {
				
				_myScore += 16;

				_listMyDinoTray.Remove (_listMyDinoTray [i+3]);
				_listMyDinoTray.Remove (_listMyDinoTray [i+2]);
				_listMyDinoTray.Remove (_listMyDinoTray [i+1]);
				_listMyDinoTray.Remove (_listMyDinoTray [i]);
			}
		}

		for (int i = 0; i < _listMyDinoTray.Count - 2; i++) {
			if (_listMyDinoTray [i].name == _listMyDinoTray [i+1].name && _listMyDinoTray[i].name == _listMyDinoTray[i+2].name) {
				_myScore += 9;

				_listMyDinoTray.Remove (_listMyDinoTray [i+2]);
				_listMyDinoTray.Remove (_listMyDinoTray [i+1]);
				_listMyDinoTray.Remove (_listMyDinoTray [i]);
			}
		}

		for (int i = 0; i < _listMyDinoTray.Count-1; i++) {
			if (_listMyDinoTray [i].name == _listMyDinoTray [i+1].name) {
				_myScore += 4;

				_listMyDinoTray.Remove (_listMyDinoTray [i+1]);
				_listMyDinoTray.Remove (_listMyDinoTray [i]);
			}
		}

		foreach(Transform _myEgg in _myEggTray) {
			if (_myEgg.tag == "Egg") {
				_eggPoint += 3;
			}
		}

		foreach (Transform _dino in _listMyDinoTray) {
			if (_dino.tag == "Dino") {
				_dinoNum++;
			}
		}

		_myScore = _myScore + _dinoNum + _eggPoint;

		return _myScore;
	}

	public void UpdateScore(){
		foreach (GameObject _player in _listPlayers) {
			var _myScore = 0;
			_myScore = CalculateMyScore (_player.transform.GetChild (3), _player.transform.GetChild (4));

			if (_player.tag == "Player") {
				var _tempScript = _player.GetComponent<PlayerController> ();
				_tempScript.SetMyScore (_myScore);
				_tempScript.SetMyScoreText (_myScore);
			} else {
				var _tempScript = _player.GetComponent<COMController> ();
				_tempScript.SetMyScore (_myScore);
				_tempScript.SetMyScoreText (_myScore);
			}

		}
	}

	/// <summary>
	/// Arranges the card in hand of COMs after show card.
	/// </summary>
	/// <param name="_myDeck">My deck.</param>
	/// <param name="_myDeckPos">My deck position.</param>
	public void ArrangeCardDeckOfPlayer(Transform _myDeck, Transform _myDeckPos){
		for (int i = 0; i < 2; i++) {
			_myDeck.GetChild (i).DOMove (_myDeckPos.GetChild (i).position, 0.2f);
		}
	}

	/// <summary>
	/// Takes the dinos with card. Use for COM
	/// </summary>
	/// <param name="_dinoTray">Dino tray.</param>
	/// <param name="_dinoTrayPos">Dino tray position.</param>
	public void TakeDinosWithCard (Transform _dinoTray, Transform _dinoTrayPos){
		int _tempCardType = _cardType;
		if (_tempCardType == 12) {
			_tempCardType = 22;
		}

		string _strCardType = "" + _tempCardType;
		int _partOfDino = int.Parse (_strCardType [0].ToString ());
		int _posOfDinoTake = int.Parse (_strCardType [1].ToString ());
		int _listDinoLength = _listDinoPlayable.Count - 1;

		List<GameObject> _listDinoCanTake = new List<GameObject> ();
		GameObject _tempDino = null;
		if (_partOfDino < _listDinoPlayable.Count) {
			for (int i = 0; i < _partOfDino; i++) {

				if (_posOfDinoTake == 0) {//take at head
					_tempDino = _listDinoPlayable [_listDinoLength - i];
					//AddDinoToTray (_tempDino.transform, _dinoTray, _dinoTrayPos);
					_listDinoCanTake.Add (_tempDino);
					//_tempDino.transform.SetParent (_dinoTray);
					_listDinoPlayable.Remove (_tempDino);
				} else if (_posOfDinoTake == 1) {//take at tail
					_tempDino = _listDinoPlayable [0];
					//AddDinoToTray (_tempDino.transform, _dinoTray, _dinoTrayPos);
					_listDinoCanTake.Add (_tempDino as GameObject);

					//_tempDino.transform.SetParent (_dinoTray);
					_listDinoPlayable.Remove (_tempDino);

				} else if (_posOfDinoTake == 2) {//take both
					if (_listDinoCanTake.Count < 2) {
						_tempDino = _listDinoPlayable [_listDinoLength];
						//AddDinoToTray (_tempDino.transform, _dinoTray, _dinoTrayPos);
						//_tempDino.transform.SetParent (_dinoTray);
						_listDinoCanTake.Add (_tempDino as GameObject);
						_listDinoPlayable.Remove (_tempDino);

						_tempDino = _listDinoPlayable [0];
						//AddDinoToTray (_tempDino.transform, _dinoTray, _dinoTrayPos);
						//_tempDino.transform.SetParent (_dinoTray);
						_listDinoCanTake.Add (_tempDino as GameObject);
						_listDinoPlayable.Remove (_tempDino);
					}
				}
			}
		} else {
			int _maxDinoPlayable = _listDinoPlayable.Count;
			for (int i = 0; i < _maxDinoPlayable; i++) {
				_tempDino = _listDinoPlayable [0];
				//AddDinoToTray (_tempDino.transform, _dinoTray, _dinoTrayPos);
				_listDinoCanTake.Add (_tempDino as GameObject);
				//_tempDino.transform.SetParent (_dinoTray);
				_listDinoPlayable.Remove (_tempDino);
			}
		}

		//Arrange Dino List to make sure better dino go to first
		if(_listDinoCanTake.Count>2){
			if (_listDinoCanTake [0].name == _listDinoCanTake [_listDinoCanTake.Count - 1].name) {
				var _temp = _listDinoCanTake [0];
				_listDinoCanTake.RemoveAt (0);
				_listDinoCanTake.Insert (_listDinoCanTake.Count-1, _temp);
			}
		}

		for (int i = 0; i < _listDinoCanTake.Count; i++) {
			
			int _nextPos = _dinoTray.childCount;
			if (_nextPos > 0) {
				if (_listDinoCanTake [i].transform.name == _dinoTray.GetChild (0).name || _listDinoCanTake [i].transform.name == _dinoTray.GetChild (_nextPos - 1).name) {
					var _temp = _listDinoCanTake [i];
					_listDinoCanTake.RemoveAt (i);
					_listDinoCanTake.Insert (0, _temp);
				}
			}
		}

		foreach (GameObject _dino in _listDinoCanTake) {
			AddDinoToTray (_dino.transform, _dinoTray, _dinoTrayPos);
		}
			
		/////ReArrange Dino Tray
		for(int i=0;i<_dinoTray.childCount;i++){
			_dinoTray.GetChild (i).DOMove (_dinoTrayPos.GetChild (i).position, 1f);
		}
	}

	void AddDinoToTray(Transform _dino, Transform _trayDino, Transform _trayDinoPos){
		if (_trayDino.childCount > 0) {
			if (_dino.name == _trayDino.GetChild (0).name) {
				_dino.SetParent (_trayDino);
				_dino.SetAsFirstSibling ();
			} else if (_dino.name == _trayDino.GetChild (_trayDino.childCount - 1).name) {
				_dino.SetParent (_trayDino);
				_dino.SetAsLastSibling ();
			} else {
				if (IsDinoMorePopularInDinoBoard (_trayDino.GetChild (0).gameObject, _trayDino.GetChild (_trayDino.childCount - 1).gameObject)) {
					_dino.SetParent (_trayDino);
					_dino.SetAsFirstSibling ();
				} else {
					_dino.SetParent (_trayDino);
					_dino.SetAsLastSibling ();
				}
			}
		} else {
			_dino.SetParent (_trayDino);
			_dino.SetAsFirstSibling ();
		}

		ScaleDinoWhenMoveToTray (_dino.gameObject, _trayDinoPos.GetChild (0));
	}

	bool IsDinoMorePopularInDinoBoard(GameObject _dinoFirst, GameObject _dinoSecond){
		int _dinoFirstNum = 0;
		int _dinoSecondNum = 0;

		foreach (GameObject _dino in _listDinoPlayable) {
			if (_dino.name == _dinoFirst.name) {
				_dinoFirstNum++;
			} else if (_dino.name == _dinoSecond.name) {
				_dinoSecondNum++;
			}
		}

		if (_dinoFirstNum > _dinoSecondNum) {
			return true;
		} else {
			return false;
		}
	}

	/// <summary>
	/// Highlights the dinos with card. --> User for Player
	/// </summary>
	public void HighlightDinosWithCard(){
		
		string _strCardType = "" + _cardType;
		_chooseDinoAmount = int.Parse (_strCardType [0].ToString());
		if (_cardType == 12) {
			_chooseDinoAmount = 2;
		}
		int _partOfDino = int.Parse (_strCardType [0].ToString ());
		int _posOfDinoTake = int.Parse (_strCardType [1].ToString ());
		int _listDinoLength = _listDinoPlayable.Count - 1;
		float _posChoosenScale = 0.3f;
		GameObject _tempDino = null;

		if (_partOfDino < _listDinoPlayable.Count) {
			for (int i = 0; i < _partOfDino; i++) {
				if (_posOfDinoTake == 0) {//Highlight at head
					_tempDino = _listDinoPlayable [_listDinoLength - i];
					Vector3 _posDinoChoosen = _tempDino.transform.position;
					_posDinoChoosen.y -= _posChoosenScale;
					GameObject _tempEffect = Instantiate (_highlightEffect, _posDinoChoosen, Quaternion.identity) as GameObject;
					_listHightLightEffect.Add (_tempEffect);
				} else if (_posOfDinoTake == 1) {//Highlight at tail
					_tempDino = _listDinoPlayable [i];
					Vector3 _posDinoChoosen = _tempDino.transform.position;
					_posDinoChoosen.y -= _posChoosenScale;
					GameObject _tempEffect = Instantiate (_highlightEffect, _posDinoChoosen, Quaternion.identity) as GameObject;
					_listHightLightEffect.Add (_tempEffect);
				} else if (_posOfDinoTake == 2) {//Highlight both
					_tempDino = _listDinoPlayable [_listDinoLength - i];
					Vector3 _posDinoChoosenHead = _tempDino.transform.position;
					_posDinoChoosenHead.y -= _posChoosenScale;
					GameObject _tempEffect = Instantiate (_highlightEffect, _posDinoChoosenHead, Quaternion.identity) as GameObject;
					_listHightLightEffect.Add (_tempEffect);
					_tempDino.GetComponent<CircleCollider2D> ().enabled = true;

					_tempDino = _listDinoPlayable [0];
					Vector3 _posDinoChoosenTail = _tempDino.transform.position;
					_posDinoChoosenTail.y -= _posChoosenScale;
					GameObject _tempEffectSecond = Instantiate (_highlightEffect, _posDinoChoosenTail, Quaternion.identity) as GameObject;
					_listHightLightEffect.Add (_tempEffectSecond);
				}
				_tempDino.GetComponent<CircleCollider2D> ().enabled = true;
			}
		} else {
			_chooseDinoAmount = _listDinoPlayable.Count;
			Debug.Log (_chooseDinoAmount);
			for (int i = 0; i < _listDinoPlayable.Count; i++) {
				_tempDino = _listDinoPlayable [i];
				Vector3 _posDinoChoosen = _tempDino.transform.position;
				_posDinoChoosen.y -= _posChoosenScale;
				GameObject _tempEffect = Instantiate (_highlightEffect, _posDinoChoosen, Quaternion.identity) as GameObject;
				_listHightLightEffect.Add (_tempEffect);

				_tempDino.GetComponent<CircleCollider2D> ().enabled = true;
			}
		}
	}

	/// <summary>
	/// Scales the dino when move to tray.
	/// Smaller and Rotate belong to card tray of each player
	/// </summary>
	/// <param name="_dino">Dino.</param>
	/// <param name="_dinoTrayPos">Dino tray position.</param>
	void ScaleDinoWhenMoveToTray(GameObject _dino, Transform _dinoTrayPos){
		_dino.transform.DOScale (Vector3.one * 0.7f, 0.3f);
		if (_dinoTrayPos.tag == "Right face") {
			_dino.GetComponent<SpriteRenderer> ().flipX = true;
		} else {
			_dino.GetComponent<SpriteRenderer> ().flipX = false;
		}

		if (_turn == 1) {
			_dino.transform.DORotate (new Vector3 (0, 0, 90), 0.3f);	
		} else if (_turn == 2) {
			_dino.transform.DORotate (new Vector3 (0, 0, 180), 0.3f);
		} else if (_turn == 3) {
			_dino.transform.DORotate (new Vector3 (0, 0, -90), 0.3f);
		}
	}

	/// <summary>
	/// Gets the type of the card.
	/// </summary>
	/// <returns>The card type.</returns>
	/// <param name="_myCard">My card.</param>
	public int GetCardType(GameObject _myCard){
		string _tag = _myCard.tag;
		int _tempCardType = 0;

		switch (_tag) {

		case "HT":
			_tempCardType = 12;
			break;
		case "1H":
			_tempCardType = 10;
			break;
		case "1T":
			_tempCardType = 11;
			break;
		case "2H":
			_tempCardType = 20;
			break;
		case "2T":
			_tempCardType = 21;
			break;
		case "3H":
			_tempCardType = 30;
			break;
		case "3T":
			_tempCardType = 31;
			break;
		}

		return _tempCardType;
	}

	public void finishButton(){
		
	}

	public void CheckEndGame(Transform _playerDinoTray){
		
		if (CheckEnd (_playerDinoTray)) {
			_endPannel.SetActive (true);
			var _winPannel = _endPannel.transform.GetChild (0).gameObject;
			_isStarPlay = false;
			_winPannel.SetActive (true);
			if (CheckWinCondition (_playerDinoTray)) {
				_winPannel.transform.GetChild (0).gameObject.SetActive (true);
			} else {
				_winPannel.transform.GetChild (1).gameObject.SetActive (true);
			}

		}

	}

	bool CheckWinCondition(Transform _playerDinoTray){
		int _playerScore = _listPlayers [0].GetComponent<IScore> ().GetMyScore ();
		bool _win = true;

		if (_playerDinoTray.parent.name == "Player") {
			for (int i = 0; i < _playerDinoTray.childCount - 4; i++) {
				if (_playerDinoTray.GetChild (i).name == _playerDinoTray.GetChild (i + 1).name && _playerDinoTray.GetChild (i).name == _playerDinoTray.GetChild (i + 2).name && _playerDinoTray.GetChild (i).name == _playerDinoTray.GetChild (i + 3).name && _playerDinoTray.GetChild (i).name == _playerDinoTray.GetChild (i + 4).name) {
					return _win;
				}
			}
		}

		for (int i = 1; i < _listPlayers.Length; i++) {
			int _comScore = _listPlayers [i].GetComponent<IScore> ().GetMyScore ();
			if (_playerScore < _comScore) {
				return false;
			}
		}

		return _win;
	}

	/// <summary>
	/// Check when stop gameplay
	/// </summary>
	/// <returns><c>true</c>, if end was checked, <c>false</c> otherwise.</returns>
	/// <param name="_playerDinoTray">Player dino tray.</param>
	bool CheckEnd(Transform _playerDinoTray){
		if (_listDinoPlayable.Count > 0) {
			for (int i = 0; i < _playerDinoTray.childCount - 4; i++) {
				if (_playerDinoTray.GetChild (i).name == _playerDinoTray.GetChild (i + 1).name && _playerDinoTray.GetChild (i).name == _playerDinoTray.GetChild (i + 2).name && _playerDinoTray.GetChild (i).name == _playerDinoTray.GetChild (i + 3).name && _playerDinoTray.GetChild (i).name == _playerDinoTray.GetChild (i + 4).name) {
					return true;
				}
			}
		} else {
			return true;
		}

		return false;
	}

	/// <summary>
	/// Lists the dino playable remove dino choosen by player.
	/// Use when put dino in player's dino tray
	/// </summary>
	/// <param name="_dino">Dino.</param>
	public void ListDinoPlayableRemoveDinoChoosenByPlayer(GameObject _dino){
		_listDinoPlayable.Remove(_dino);
	}

	/// <summary>
	/// Gets the amount of choosen dino.
	/// Use at PlayerController
	/// Use to know how many dinos will be choosen
	/// </summary>
	/// <returns>The amount of choosen dino.</returns>
	public int GetAmountOfChoosenDino(){
		return _chooseDinoAmount;
	}

	public void SoundCardPlay(){
		_audioSource.PlayOneShot (_audioClipCard);
	}

	public void ClickButtonHint(){
		if (SystemController.instance != null) {
			SystemController.instance.ClickTutorialButton ();
		}
	}

	public void CLickLoadScene(int _scene){
		SceneManager.LoadScene (0);
	}

	public void SetAmountOfChoosenDino(int _value){
		_chooseDinoAmount = _value;
	}

	public bool GetIsStarPlay(){
		return _isStarPlay;
	}

	public void SetIsStarPlay(bool _value){
		_isStarPlay = _value;
	}

	public bool GetIsCompleteAction(){
		return _isCompleteAction;
	}

	public void SetIsCompleteAction(bool _value){
		_isCompleteAction = _value;
	}

	public bool GetIsCompleteDrawCard(){
		return _isCompleteDrawCard;
	}

	public void SetIsCompleteDrawCard(bool _value){
		_isCompleteDrawCard = _value;
	}

	public bool GetIsPlayerChoosenDino(){
		return _isPlayerChoosenDino;
	}

	public void SetIsPlayerChoosenDino(bool _value){
		_isPlayerChoosenDino = _value;
	}

	public void SetListDinoPlayaple(List<GameObject> _myList){
		_listDinoPlayable = new List<GameObject> (_myList);
	}

	public void RemoveAllChoosenHighlightEffect(){
		foreach (GameObject _effect in _listHightLightEffect) {
			Destroy (_effect);
		}
	}

	public bool GetIsCompleteMagic(){
		return _isCompleteMagic;
	}

	public void SetIsCompleteMagic(bool _value){
		_isCompleteMagic = _value;
	}

	public void SetIsStarGiveCard(bool _value){
		_isStarGiveCard = _value;
	}

	public bool GetIsStarGiveCard(){
		return _isStarGiveCard;
	}
}
