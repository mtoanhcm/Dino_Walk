using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class BoardDinoController : MonoBehaviour {

	[SerializeField] private GameObject[] _dinoData;
	[SerializeField] private Transform _spawnPos;

	private List<Transform> _boardDinoLinePos;
	private List<Transform> _spawnDinoPosition;
	private List<GameObject> _listDinosPlayable;

	void Awake(){
		_listDinosPlayable = new List<GameObject> ();

		//Edited
		CreateSpawnDinoPosition ();

		CreateListOfDinoLinesPositionOnBoard ();
		CreateListDinoPlayable ();
	}
		
	void Start () {
		//CreateDinosOnBoard ();
		SetListDinoInToSpawnPos();
		MoveDinosOnBoard ();
		GameplayController.instance.SetListDinoPlayaple (_listDinosPlayable);
	}
	
	void CreateListOfDinoLinesPositionOnBoard(){
		var _temp = transform.GetComponentsInChildren<Transform> ();
		_boardDinoLinePos = new List<Transform> (_temp);
		_boardDinoLinePos.RemoveAt (0);
	}

	//Edited
	void CreateSpawnDinoPosition(){
		var _temp = _spawnPos.GetComponentsInChildren<Transform> ();
		_spawnDinoPosition = new List<Transform> (_temp);
		_spawnDinoPosition.RemoveAt (0);
	}
		
	void CreateListDinoPlayable(){
		for (int i = 0; i < 9; i++) {//9
			for (int j = 0; j < 7; j++) {//7
				GameObject _dino = Instantiate (_dinoData [j], _spawnPos.position, Quaternion.identity) as GameObject;
				_listDinosPlayable.Add (_dino);
			}
		}

		SuffleDeck (_listDinosPlayable);
	}

	//Edited
	void SetListDinoInToSpawnPos(){
		for (int i = 0; i < _listDinosPlayable.Count; i++) {
			_listDinosPlayable [i].transform.position = _spawnDinoPosition [i].position;
			//_listDinosPlayable [i].transform.SetParent (_spawnPos);

			Destroy (_spawnDinoPosition [i].gameObject);
		}

		Destroy (_spawnDinoPosition [_spawnDinoPosition.Count-1].gameObject);
		_spawnDinoPosition.Clear ();
	}

	void SuffleDeck(List<GameObject> _deck){
		for (int i = _deck.Count - 1; i > 0; i--) {
			int _random = Random.Range(0,i+1);
			var _temp = _deck[i];
			_deck[i] = _deck[_random];
			_deck[_random] = _temp;
		}
	}

	/*
	void CreateDinosOnBoard(){
		for (int i = 0; i < _listDinosPlayable.Count; i++) {
			_listDinosPlayable [i].transform.position = _boardDinoLinePos [i].position;
			_listDinosPlayable [i].transform.SetParent (transform);

			if (_boardDinoLinePos[i].tag == "Right face") {
				_listDinosPlayable[i].GetComponent<SpriteRenderer> ().flipX = true;
			}

			Destroy (_boardDinoLinePos [i].gameObject);
		}

		Destroy (_boardDinoLinePos [_boardDinoLinePos.Count-1].gameObject);
		_boardDinoLinePos.Clear ();
	}
	*/

	void MoveDinosOnBoard(){
		for (int i = 0; i < _listDinosPlayable.Count; i++) {
			var _dino = _listDinosPlayable [i].transform;
			var _dinoScript = _dino.GetComponent<DinoController> ();
			float _randomTime = Random.Range (1f, 2f);
			_dino.DOMove(_boardDinoLinePos [i].position,_randomTime).OnComplete(() => _dinoScript.SetPosInMainBoard(_dino.position));
			_dino.SetParent (transform);

			if (_boardDinoLinePos[i].tag == "Right face") {
				_dino.GetComponent<SpriteRenderer> ().flipX = true;
			}

			Destroy (_boardDinoLinePos [i].gameObject);
		}

		Destroy (_boardDinoLinePos [_boardDinoLinePos.Count-1].gameObject);
		Destroy (_spawnPos.gameObject);
		_boardDinoLinePos.Clear ();

		GameplayController.instance.SetIsStarGiveCard (true);
	}
}
