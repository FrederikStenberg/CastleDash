using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneChanger : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    
	}

    string currentScene;
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.D))
        {
            currentScene = SceneManager.GetActiveScene().name;

            if (currentScene == "Game")
            {
                SceneManager.LoadScene(1);
            }
            else
            {
                SceneManager.LoadScene(0);
            }
        }
            
	}
}
