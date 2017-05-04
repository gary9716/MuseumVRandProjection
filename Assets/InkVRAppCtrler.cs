using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InkVRAppCtrler : MonoBehaviour {

    public enum AppMode
    {
        VR,
        Video
    };

    public GameObject entranceSceneUI;
    public AppMode mode;
    public string mainSceneName;

    //parameters put here
    

	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this);	
	}
	

    public void StartMainApp(AppMode mode)
    {
        this.mode = mode;
        GoToMainScene();
    }

    public void StartVR() {
        StartMainApp(AppMode.VR);
    }

    public void StartPlayingVideo() {
        StartMainApp(AppMode.Video);
    }


    public void GoToMainScene()
    {
        entranceSceneUI.SetActive(false);
        SceneManager.LoadScene(mainSceneName, LoadSceneMode.Single);
    }

    public void Quit()
    {
        //If we are running in a standalone build of the game
#if UNITY_STANDALONE
        //Quit the application
        Application.Quit();
#endif

        //If we are running in the editor
#if UNITY_EDITOR
        //Stop playing the scene
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }




}
