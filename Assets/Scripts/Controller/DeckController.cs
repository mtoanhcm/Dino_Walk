using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class DeckController : MonoBehaviour {

	[SerializeField] private GameObject[] _cardDinoData;
	[SerializeField] private GameObject[] _cardMagicData;
	[SerializeField] private GameObject[] _cardPlayers;

	private List<GameObject> _listCardPlayable;
	private List<GameObject> _theDeck;
	private List<GameObject> _listMagicCards;

	void Awake(){
		_listCardPlayable = new List<GameObject> ();
		_theDeck = new List<GameObject> ();
		_listMagicCards = new List<GameObject> ();

		BuildDeck ();
	}
		
	void Start () {
		CreateListCardsDinoPlayable ();
		StartCoroutine (CheckHowIsStartGiveCard ());
	}

	IEnumerator CheckHowIsStartGiveCard(){
		bool _isStartGiveCard = GameplayController.instance.GetIsStarGiveCard ();
		yield return new WaitForSeconds (1f);
		if (_isStartGiveCard) {
			_isStartGiveCard = false;
			StartCoroutine (GiveCardsForPlayerAtStart (GameplayController.instance.GetTurn()));
			yield break;
		}

		StartCoroutine (CheckHowIsStartGiveCard ());
	}

	void BuildDeck(){
		for (int i = 0; i < 6; i++) {
			for (int j = 0; j < _cardDinoData.Length; j++) {
				_theDeck.Add (_cardDinoData[j]);
			}
		}

		CreateMagicCard ();

		SuffleDeck (_theDeck);

		AddMagicCardToDeck ();

	}

	void CreateMagicCard(){
		//i<5
		for (int i = 0; i < 5; i++) {
			if (i < 3) {//i<3
				_listMagicCards.Add (_cardMagicData [0]);//Card Volcano
				_listMagicCards.Add (_cardMagicData [1]);//Card Rope
			}

			_listMagicCards.Add (_cardMagicData [2]);//Card Egg
		}
	}

	void AddMagicCardToDeck(){
		foreach (GameObject _card in _listMagicCards) {
			var _randomIndex = Random.Range (12, _theDeck.Count);
			_theDeck.Insert (_randomIndex, _card);
			//_theDeck.Insert (12, _card);
		}
	}
	
	void CreateListCardsDinoPlayable(){
		Vector3 _tempPos = transform.position;
		_tempPos.y += 0.6f;
		_tempPos.x += 0.05f;

		foreach(GameObject _card in _theDeck){
			GameObject _tempCard = Instantiate (_card, _tempPos, Quaternion.identity) as GameObject;
			_tempCard.transform.SetParent (transform);
			_listCardPlayable.Add (_tempCard);

			_tempPos.x += 0.01f;
		}

	}

	void SuffleDeck(List<GameObject> _deck){
		for (int i = _deck.Count - 1; i > 0; i--) {
			int _random = Random.Range(0,i+1);
			var _temp = _deck[i];
			_deck[i] = _deck[_random];
			_deck[_random] = _temp;
		}
	}

	IEnumerator GiveCardsForPlayerAtStart(int _turn){
		if (_turn > 3) {
			_turn = 0;
			GameplayController.instance.SetTurn (_turn);
			GameplayController.instance.SetIsStarPlay (true);

			yield return new WaitForSeconds (0.4f);
			StartCoroutine (GiveCardsForPlayerAtEachTurn ());
			yield break;
		}

		var _tempPlayerDeck = _cardPlayers [_turn].transform.GetChild (1);
		var _tempPlayerDeckPos = _cardPlayers [_turn].transform.GetChild (0);
		var _tempCardPos = _tempPlayerDeckPos.GetComponentsInChildren<Transform> ();

		for (int i = 1; i < _tempCardPos.Length-1; i++) {
			yield return new WaitForSeconds (0.4f);
			GiveCardsForEachPlayers (_tempPlayerDeck, _tempCardPos [i], _turn);
		}

		_turn++;
		GameplayController.instance.SetTurn (_turn);
		StartCoroutine (GiveCardsForPlayerAtStart (_turn));
	}

	IEnumerator GiveCardsForPlayerAtEachTurn(){
		if (_listCardPlayable.Count < 1) {
			GameplayController.instance.SetIsStarPlay (false);
			yield break;
		}

		yield return new WaitForSeconds (1.5f);
		int _turn = GameplayController.instance.GetTurn ();

		var _isCompleteDraw = GameplayController.instance.GetIsCompleteDrawCard ();

		if (!_isCompleteDraw) {
			
			var _tempPlayerDeck = _cardPlayers [_turn].transform.GetChild (1);
			var _tempPlayerDeckPos = _cardPlayers [_turn].transform.GetChild (0);

			if (gameObject.transform.childCount != 0) {
			GiveCardsForEachPlayers (_tempPlayerDeck, _tempPlayerDeckPos.GetChild(2), _turn);
			_isCompleteDraw = true;
			GameplayController.instance.SetIsCompleteDrawCard (_isCompleteDraw);
			}
		}


		StartCoroutine(GiveCardsForPlayerAtEachTurn());
	}

	void FlipCard(Transform _card){
		Vector3 _tempRotate = new Vector3 (0, 180, 0);
		_card.DORotate (_tempRotate, 0.5f);
	}

	void SetActiveCardColliderForClickChoose(GameObject _card){
		_card.GetComponent<BoxCollider2D> ().enabled = true;
	}

	void GiveCardsForEachPlayers(Transform _playerDeck, Transform _cardPos,int _myTurn){
		var _temp = _listCardPlayable [0];
		float _scaleTime = 1f;
		GameplayController.instance.SoundCardPlay ();

		if (_myTurn == 0) {
			_temp.transform.DOMove (_cardPos.position, _scaleTime).OnComplete (() => FlipCard (_temp.transform));

		} else if (_myTurn == 1) {
			_temp.transform.DOMove (_cardPos.position, _scaleTime);
			_temp.transform.DORotate (new Vector3 (0, 0, 90), _scaleTime);
			_temp.transform.localScale = Vector3.one * 0.8f;
		} else if (_myTurn == 2) {
			_temp.transform.DOMove (_cardPos.position, _scaleTime);
			_temp.transform.DORotate (new Vector3 (0, 0, 180), _scaleTime);
			_temp.transform.localScale = Vector3.one * 0.8f;
		} else if (_myTurn == 3) {
			_temp.transform.DOMove (_cardPos.position, _scaleTime);
			_temp.transform.DORotate (new Vector3 (0, 0, -90), _scaleTime);
			_temp.transform.localScale = Vector3.one * 0.8f;
		}

		_temp.transform.SetParent (_playerDeck);
		_listCardPlayable.RemoveAt (0);
	}
		
}
