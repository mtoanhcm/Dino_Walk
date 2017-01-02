using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SoundController : MonoBehaviour {

	[SerializeField] private Button _soundOn = null;
	[SerializeField] private Button _soundOff = null;
	private bool _isMute;

	// Use this for initialization
	void Start () {
		if (SystemController.instance != null) {
			_isMute = SystemController.instance.audioSource.mute;
		}
		SetStateSoundButton ();
	}
		
	public void ClickToggleMusich(){
		_isMute = !_isMute;
		if (SystemController.instance != null) {
			SystemController.instance.audioSource.mute = _isMute;
		}

		if (GameplayController.instance != null) {
			GameplayController.instance._audioSource.mute = _isMute;
		}

		SetStateSoundButton ();
	}

	void SetStateSoundButton(){
		if (_isMute) {
			_soundOn.gameObject.SetActive (false);
			_soundOff.gameObject.SetActive (true);
		} else {
			_soundOn.gameObject.SetActive (true);
			_soundOff.gameObject.SetActive (false);
		}
	}
}
