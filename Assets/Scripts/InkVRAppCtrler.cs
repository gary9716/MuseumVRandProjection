using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Valve.VR;
public enum AppMode
{
    VR = 0,
    Video = 1
};

public enum InputVCRMode
{
    Passthru = 0,   // normal input
    Record = 1,
    Playback = 2,
    Pause = 3
};


public class InkVRAppCtrler : MonoBehaviour {
    
    [Header("entrance scene")]
    public GameObject entranceSceneUI;
    public AppMode mode;
    public string mainSceneName;
    public bool disableVRExplicitly;

    [Header("main scene")]
    public InkVR_InputVCRController vcrCtrler;
    public DemoSceneUIManager uiManager;
    
    //if we need any parameter, put them here and make them public
    

	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this);	
	}

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Quit();
        }
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
        Destroy(entranceSceneUI);
        
        if(disableVRExplicitly && mode == AppMode.Video)
        {
            SteamVR.SafeDispose();
            SteamVR.enabled = false;
        }
        
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        SceneManager.LoadSceneAsync(mainSceneName);
        
        //less coupling is better.
        //so just assign the value and let the object do the work itself

    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if(scene.name == mainSceneName)
        {
            initInMainScene();

            uiManager.SetAppModeTxt(mode);

            //if (mode == AppMode.Video)
                //vcrCtrler.SetAutoRandomPlayback(true);

        }


    }
    
    void initInMainScene()
    {
        vcrCtrler = FindObjectOfType<InkVR_InputVCRController>();
        uiManager = FindObjectOfType<DemoSceneUIManager>();
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
