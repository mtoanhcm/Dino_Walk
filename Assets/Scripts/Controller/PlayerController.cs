using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class PlayerController : MonoBehaviour, IScore {

	[SerializeField] private Transform _myDeck;
	[SerializeField] private Transform _myDeckPos;
	[SerializeField] private Transform _myDinoTray;
	[SerializeField] private Transform _myDinoTrayPos;
	[SerializeField] private Transform[] _myEnemyDinoTrays;
	[SerializeField] private GameObject _firstChoosenFrame;
	[SerializeField] private GameObject _lastChoosenFrame;
	[SerializeField] private Transform _choosenRectList;
	[SerializeField] private TextMesh _myScoreText;

	private bool _isDrawCard;
	private bool _isHighlight;
	private bool _isHaveMagic;
	private bool _isRopeCard;
	private bool _isCompleteShowCard;
	private GameObject _firstChoosenRect;
	private GameObject _lastChoosenRect;
	private GameObject _myMagicCard;
	private int _score;

	void Awake(){
		_firstChoosenRect = null;
		_lastChoosenRect = null;
		_score = 0;
		_myScoreText.GetComponent<MeshRenderer> ().sortingOrder = 6;
	}

	void Start () {
		StartCoroutine (Play ());
		_isHighlight = true;

	}

	IEnumerator Play(){
		if (GameplayController.instance.GetIsStarPlay ()) {
			if (GameplayController.instance.GetTurn () == 0) {
				DrawnCard ();
				yield return new WaitForSeconds (0.6f);
				StartCoroutine(DetectMagicCard ());
				yield return new WaitForSeconds (0.5f);
				if (!_isHaveMagic) {
					HighlightDinosWithCard ();
				} else {
					//EnablePosToPutDino ();
					//EnablePosToPutDino use at GameplayController script to make sure enable after card show
					ContinueChooseDino ();
				}
				yield return new WaitForSeconds (2f);
				EndTurn ();
			}
		}

		yield return new WaitForSeconds (0.1f);
		StartCoroutine (Play ());
	}

	void EndTurn(){
		bool _isCompelete = GameplayController.instance.GetIsCompleteAction ();
		if (_isCompelete) {
			GameplayController.instance.ScaleAll ();
			_score = GameplayController.instance.CalculateMyScore (_myDinoTray, transform.GetChild (4));
			_myScoreText.text = "" + _score;
			int _temp = GameplayController.instance.GetTurn ();
			_temp++;
			GameplayController.instance.CheckEndGame (_myDinoTray);
			GameplayController.instance.SetIsCompleteAction (!_isCompelete);
			GameplayController.instance.SetTurn (_temp);
			GameplayController.instance.ToggleCollider (false);

			_isDrawCard = false;
			_isHighlight = true;
			_isHaveMagic = false;
			_isCompleteShowCard = false;
		}
	}

	void DrawnCard(){
		if (!_isDrawCard) {
			GameplayController.instance.SetIsCompleteDrawCard (false);
			GameplayController.instance.TimeCountDown (transform);

			_isDrawCard = true;
		}
	}

	IEnumerator DetectMagicCard(){
		yield return new WaitForSeconds (0.5f);
		foreach (Transform _card in _myDeck) {
			if (_card.tag == "Magic") {
				_isHaveMagic = true;
				_isCompleteShowCard = true;
				_myMagicCard = _card.gameObject;

				GameplayController.instance.ShowCard (_myMagicCard);
				GameplayController.instance.SetIsPlayerChoosenDino (true);
				GameplayController.instance.SetAmountOfChoosenDino (1);
			}
		}

		if (!_isCompleteShowCard) {
			foreach(Transform _card in _myDeck){
				if (_myDeck.childCount > 2) {
					_card.GetComponent<BoxCollider2D> ().enabled = true;
					_isCompleteShowCard = true;
				}
			}
		}
	}

	void HighlightDinosWithCard(){
		bool _isChooseDinos = GameplayController.instance.GetIsPlayerChoosenDino ();

		if (_isChooseDinos) {
			GameplayController.instance.ArrangeCardDeckOfPlayer (_myDeck, _myDeckPos);
			if (_isHighlight) {
				GameplayController.instance.HighlightDinosWithCard ();
				_isHighlight = false;
			}

			EnablePosToPutDino ();
			ContinueChooseDino ();
		}
	}

	public void EnablePosToPutDino(){
		if (_myDinoTray.childCount < _myDinoTrayPos.childCount) {
			if(_firstChoosenRect == null){
				Vector3 _firstTrayPos = _myDinoTrayPos.GetChild (0).position;
				_firstChoosenRect = Instantiate (_firstChoosenFrame, _firstTrayPos, Quaternion.identity) as GameObject;
				_firstChoosenRect.transform.SetParent (_myDinoTray);
				_firstChoosenRect.transform.SetAsFirstSibling ();
			}

			if (_lastChoosenRect == null) {
				if (_myDinoTray.childCount > 1) {
					Vector3 _lastChildPos = _myDinoTrayPos.GetChild (_myDinoTray.childCount).position;
					_lastChoosenRect = Instantiate (_lastChoosenFrame, _lastChildPos, Quaternion.identity) as GameObject;
					_lastChoosenRect.transform.SetParent (_myDinoTray);
				}
			}
		}
	}

	void ContinueChooseDino(){
		var _playerDinoTray = transform.GetChild (3);
		var _playerDinoTrayPos = transform.GetChild(2);
		bool _isChoose = false;
		bool _isCompleteMagic = GameplayController.instance.GetIsCompleteMagic ();
		int _num = GameplayController.instance.GetAmountOfChoosenDino ();

		//Complete Choose
		if (_num <= 0 || _isCompleteMagic) {
			_isCompleteMagic = false;
			GameplayController.instance.SetIsCompleteMagic (_isCompleteMagic);
			GameplayController.instance.SetIsPlayerChoosenDino (_isChoose);
			StartCoroutine(ReArrangeDinoTray(_playerDinoTray,_playerDinoTrayPos));
			GameplayController.instance.SetIsCompleteAction (true);
			GameplayController.instance.RemoveAllChoosenHighlightEffect ();

			if (_choosenRectList.childCount > 0) {
				foreach (Transform _rect in _choosenRectList) {
					Destroy (_rect.gameObject);
				}
			}
		} else {
			for (int i = 0; i < _playerDinoTray.childCount-1; i++) {
				_playerDinoTray.GetChild (i+1).DOMove (_playerDinoTrayPos.GetChild (i + 1).position, 0.3f);
			}
		}
	}

	IEnumerator ReArrangeDinoTray(Transform _dinoTray, Transform _dinoTrayPos){
		Destroy (_firstChoosenRect);
		Destroy (_lastChoosenRect);
		yield return new WaitForSeconds (0.1f);

		for (int i = 0; i < transform.GetChild (3).childCount; i++) {
			_dinoTray.GetChild (i).DOMove (_dinoTrayPos.GetChild (i).position, 0.3f);
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
		return _myEnemyDinoTrays;
	}
}
