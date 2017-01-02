using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour {

	public void ClickChangeScene(int _scene){
		SceneManager.LoadScene(_scene);
	}

	public void ClickHint(){
		SystemController.instance.ClickTutorialButton ();
	}
}
