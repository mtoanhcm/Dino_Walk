using UnityEngine;
using System.Collections;

public class CardController : MonoBehaviour {

	private bool _isMouseOver;

	void OnMouseOver(){
		if (!_isMouseOver) {
			MyMoveWhenMouseHover (1);
			_isMouseOver = !_isMouseOver;
		}
	}

	void OnMouseExit(){
		MyMoveWhenMouseHover (-1);
		_isMouseOver = !_isMouseOver;
	}

	void MyMoveWhenMouseHover(int _move){
		Vector3 _temp = transform.position;
		_temp.y += 0.2f * _move;
		transform.position = _temp;
	}

	void OnMouseDown(){
		GameplayController.instance.ShowCard (gameObject);
		GameplayController.instance.SetIsPlayerChoosenDino (true);
	}
}
