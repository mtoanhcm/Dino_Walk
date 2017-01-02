using UnityEngine;
using System.Collections;
using DG.Tweening;

public class DinoController : MonoBehaviour {

	[SerializeField] private bool _isRightPos;
	[SerializeField] private bool _isInTray;

	private Vector3 _mousePos;

	private Vector3 _posInMainBoard;
	private Transform _playerDinoTray;
	private Transform _choosenRect;


	private Vector3 _oldPosInTray;


	void OnMouseDown(){
		if (transform.parent.tag != "Board Dino") {
			_oldPosInTray = transform.position;
			transform.parent.GetComponent<BoxCollider2D> ().enabled = true;
			_isInTray = true;
		}

	}

	void OnMouseDrag(){
		_mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		transform.position = new Vector3 (_mousePos.x, _mousePos.y, 0);

	}

	void OnMouseUp(){
		if (!_isRightPos) {
			if (_isInTray) {
				transform.DOMove (_oldPosInTray, 0.3f);
			} else {
				transform.DOMove (_posInMainBoard, 0.3f);
			}
		} else {
			PutDinoInPlayerTray ();
		}

		if (_isInTray) {
			if (transform.parent.parent.tag != "Player") {
				transform.parent.GetComponent<BoxCollider2D> ().enabled = false;
			}
		}

		//GameplayController.instance.ScaleAll ();
	}

	void OnTriggerStay2D(Collider2D _target){

		if (_target.tag == "First Choosen Rect" || _target.tag == "Last Choosen Rect") {
			_playerDinoTray = _target.transform.parent;
			_choosenRect = _target.transform;
			_isRightPos = true;
			ScaleDinoWhenEnterTray (0.7f);
		}
	}

	void OnTriggerExit2D(Collider2D _target){

		if (_target.tag == "First Choosen Rect" || _target.tag == "Last Choosen Rect") {
			_isRightPos = false;
			_playerDinoTray = null;
			_choosenRect = null;
			ScaleDinoWhenEnterTray (1.0f);
		}

		if (_target.tag == "Choosen Rect") {
			_isRightPos = false;
		}
	}

	void ScaleDinoWhenEnterTray(float _numScale){
		transform.DOScale (Vector3.one * _numScale, 0.3f);
	}

	void PutDinoInPlayerTray(){
		int _numDinoChoose = GameplayController.instance.GetAmountOfChoosenDino ();
		_numDinoChoose--;
		GameplayController.instance.SetAmountOfChoosenDino (_numDinoChoose);
		GameplayController.instance.ListDinoPlayableRemoveDinoChoosenByPlayer (gameObject);

		transform.DOMove (_choosenRect.position, 0.3f);
		transform.SetParent (_playerDinoTray);
		if(_choosenRect.tag == "First Choosen Rect"){
			transform.SetSiblingIndex (1);//0 is Choosen Rect
		} else if(_choosenRect.tag =="Last Choosen Rect"){
			int _num = _playerDinoTray.childCount;
			transform.SetSiblingIndex (_num - 2);
		}

		transform.GetComponent<CircleCollider2D> ().enabled = false;
	}

	public void SetPosInMainBoard(Vector3 _value){
		_posInMainBoard = _value;
	}
}
