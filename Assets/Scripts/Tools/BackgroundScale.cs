using UnityEngine;
using System.Collections;

public class BackgroundScale : MonoBehaviour {
	
	// Use this for initialization
	void Awake () {
		ScaleBackground ();
	}
	
	void ScaleBackground(){
		SpriteRenderer _sr = GetComponent<SpriteRenderer> ();
		Vector3 _temp = transform.localScale;

		//Sprite's bounds
		float _width = _sr.bounds.size.x;
		float _height = _sr.bounds.size.y;

		float _worldHeight = Camera.main.orthographicSize * 2;
		float _worldWidth = _worldHeight * Screen.width / Screen.height;

		_temp.y = _worldHeight / _height;
		_temp.x = _worldWidth / _width;

		transform.localScale = _temp;
	}
}
