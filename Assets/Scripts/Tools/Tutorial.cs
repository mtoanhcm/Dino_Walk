using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour {

	[SerializeField] SpriteRenderer[] _tutorialPic;
	[SerializeField] Button[] _navigatorButton;
	private int _indexOfPic;

	void Start(){
		ShowTutorial (0);
		CheckNavigatorButtonStatus ();
	}

	/// <summary>
	/// Set layer of current tutorial pic is higher than the others
	/// </summary>
	/// <param name="_index">Index.</param>
	void ShowTutorial(int _index){
		for (int i = 0; i < _tutorialPic.Length; i++) {
			if (i == _index) {
				_tutorialPic [i].sortingOrder = 20;
			} else {
				_tutorialPic [i].sortingOrder = 19;
			}
		}
	}

	/// <summary>
	/// Checks the navigator button status.
	/// 0 is button left
	/// 1 is button right
	/// </summary>
	void CheckNavigatorButtonStatus(){
		if (_indexOfPic <= 0) {
			_indexOfPic = 0;
			_navigatorButton [0].interactable = false;
			_navigatorButton [1].interactable = true;
		} else if (_indexOfPic >= _tutorialPic.Length -1) {
			_indexOfPic = _tutorialPic.Length - 1;
			_navigatorButton [1].interactable = false;
			_navigatorButton [0].interactable = true;
		} else {
			_navigatorButton [0].interactable = true;
			_navigatorButton [1].interactable = true;
		}
	}

	/// <summary>
	/// Checks the button press.
	/// Use at click Left or Right Button of Tutorial
	/// True is Right and Left is false
	/// </summary>
	/// <param name="_direct">true is "right" and false is "left".</param>
	public void CheckButtonPress(bool _direct){
		if (_direct) {
			_indexOfPic++;
		} else {
			_indexOfPic--;
		}

		CheckNavigatorButtonStatus ();
		ShowTutorial (_indexOfPic);
		Debug.Log (_indexOfPic);
	}

	public void ClickButtonHome(){
		Time.timeScale = 1;
		gameObject.SetActive (false);
		_indexOfPic = 0;
		ShowTutorial (_indexOfPic);
		CheckNavigatorButtonStatus ();
	}
}
