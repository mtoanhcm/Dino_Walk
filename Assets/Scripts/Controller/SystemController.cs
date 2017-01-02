using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SystemController : MonoBehaviour {
	
	public static SystemController instance;

	[SerializeField] private GameObject _tutorialPannel;

	public AudioSource audioSource;
	public int totalPoint;

	void MakeSingleton(){
		if (instance != null) {
			Destroy (gameObject);
		} else if (instance == null) {
			instance = this;
			DontDestroyOnLoad (instance);
		}
	}

	void Awake () {
		MakeSingleton ();
		totalPoint = 0;
	}

	void Start(){
		audioSource.Play ();
	}

	public void ClickTutorialButton(){
		_tutorialPannel.SetActive (true);
		Time.timeScale = 0;
	}

}
