using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using System.Linq;
using UnityEngine;
using LitJson;

public class InkVR_InputVCRController : MonoBehaviour {

    public string recordFolderName;
    public List<InputVCR> allVCR;

    public SteamVR_TrackedObject leftCtrler;
    public SteamVR_TrackedObject rightCtrler;
    public SteamVR_TrackedObject head;
    public SteamVR_Camera vrCam;

    public string defaultRecordName;
    public bool enableKeyCtrlFunc;

    public DemoSceneUIManager uiManager;
    public Transform vcrPlaybackModeParent;

    Dictionary<string, InputVCR> vcrDict;
    Dictionary<string, Recording> recordingDict;
    string recordingsPath;
    InputVCR vcrEventNotifier;


    private bool isRecording = false;
    private bool isPlaying = false;

    string fileExtensionName = ".json";

    enum VCRAction
    {
        Stop,
        Pause,
        Play,
        Record,
        NewRecording
    }

    [HideInInspector]
    public bool chooseCurrentRecordingFirst;
    Recording[] recordingChosenOrder = new Recording[2];

    void DoToAllVCR(VCRAction action)
    {
        foreach(InputVCR vcr in allVCR)
        {
            if(action == VCRAction.Stop)
            {
                vcr.Stop(); //stop playback or recording
            }
            else if (action == VCRAction.Pause)
            {
                vcr.Pause(); //pause playback or recording
            }
            else if(action == VCRAction.Play)
            {
                PlayRecordsBasedOnChosenOrder(vcr);
            }
            else if(action == VCRAction.Record)
            {
                vcr.Record(); //start recording and if it is recording, it would append to this record 
            }
            else if(action == VCRAction.NewRecording)
            {
                vcr.NewRecording(); //start a whole new recording and old recording would be dropped
            }
            
        }

        print("start vcr action:" + action.ToString());

        if(action == VCRAction.Play)
        {
            isPlaying = true;
        }
        else if(action == VCRAction.Record || action == VCRAction.NewRecording)
        {
            isRecording = true;
        }
        else if (action == VCRAction.Pause || action == VCRAction.Stop)
        {
            isPlaying = false;
            isRecording = false;
        }

        if (uiManager != null)
            uiManager.SetVCRStateTxt(GetVCRMode());

    }

    void PlayRecordsBasedOnChosenOrder(InputVCR vcr)
    {
        Recording recording = null;
        Recording[] recordingChosenOrder = null;

        if (chooseCurrentRecordingFirst)
        {
            recordingChosenOrder[0] = vcr.GetRecording();
            recordingChosenOrder[1] = recordingDict[vcr.vcrID];
        }
        else
        {
            recordingChosenOrder[1] = vcr.GetRecording();
            recordingChosenOrder[0] = recordingDict[vcr.vcrID];
        }

        recording = recordingChosenOrder[0];
        if (recording != null)
            vcr.Play(recording, 0);
        else
        {
            recording = recordingChosenOrder[1];
            if (recording != null)
                vcr.Play(recording, 0);
            else
            {
                Debug.LogWarning("one of recording is null");
            }
        }
    }


    public void TogglePlaying()
    {
        if (isPlaying)
        {
            // stop
            DoToAllVCR(VCRAction.Stop);
        }
        else
        {
            // play
            DoToAllVCR(VCRAction.Play);
        }
    }

    string currentRecordingName = "";

    public void ToggleRecording()
    {
        if (isRecording)
        {
            DoToAllVCR(VCRAction.Stop);
            SaveRecord(currentRecordingName);
        }
        else
        {
            DoToAllVCR(VCRAction.NewRecording);
            currentRecordingName = DateTime.Now.ToString("yyyy-MM-dd_H;mm;ss") + "_vcrRecord";
        }
        
    }
    
    public InputVCRMode GetVCRMode()
    {
        if (allVCR != null && allVCR.Count > 0)
            return allVCR[0].mode;
        else
            return InputVCRMode.Passthru;
    }

    /*
    public void initAppCtrler(InkVRAppCtrler appCtrler)
    {
        this.appCtrler = appCtrler;
    }
    */

    private void Awake()
    {
        if(allVCR.Count == 0)
        {
            Debug.LogWarning("no vcr set, disabled");
            this.enabled = false;
            return;
        }
        
        if (string.IsNullOrEmpty(recordFolderName))
        {
            recordFolderName = "inputVCRRecords";
        }

        string folderPath = Application.dataPath + "/" + recordFolderName;
        //print(folderPath);

        if(!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        recordingsPath = folderPath;

        vcrDict = new Dictionary<string, InputVCR>();
        recordingDict = new Dictionary<string, Recording>();
        foreach (InputVCR vcr in allVCR)
        {
            vcrDict.Add(vcr.vcrID, vcr);
        }
        
        vcrEventNotifier = allVCR[0];
        vcrEventNotifier.finishedPlayback += PlaybackEnd;
        
        DoToAllVCR(VCRAction.Stop);

    }
    
    void Update()
    {
        if(enableKeyCtrlFunc)
        {
            if (Input.GetKeyDown(KeyCode.P))
                TogglePlaying();
            else if (Input.GetKeyDown(KeyCode.R))
                ToggleRecording();
        }
        
    }
    
    public bool allVCRAreHoldingRecording()
    {
        foreach(InputVCR vcr in allVCR)
        {
            if (vcr.GetRecording() == null)
                return false;
        }

        return true;
    }

    string GetFullFilePath(string fileName)
    {
        return recordingsPath + "/" + fileName + fileExtensionName;
    }
    
    public void SaveRecord(string fileName)
    {
        if (!allVCRAreHoldingRecording())
        {
            Debug.LogWarning("some vcr weren't holding recording");
            return;
        }

        if(string.IsNullOrEmpty(fileName))
        {
            fileName = defaultRecordName;
        }

        StringBuilder sb = new StringBuilder();
        JsonWriter writer = new JsonWriter(sb);
        writer.WriteObjectStart();
        foreach (InputVCR vcr in allVCR)
        {
            writer.WritePropertyName(vcr.vcrID);
            writer.Write(vcr.GetRecording().ToString());
        }
        writer.WriteObjectEnd();
        File.WriteAllText(GetFullFilePath(fileName), writer.ToString());
    }

    public void LoadRecord(string fileName)
    {
        string jsonStr = File.ReadAllText(GetFullFilePath(fileName));
        JsonData data = JsonMapper.ToObject(jsonStr);
        recordingDict.Clear();
        foreach (InputVCR vcr in allVCR)
        {
            string recordingJsonStr = (string)data[vcr.vcrID];
            Recording recording = Recording.ParseRecording(recordingJsonStr);
            recordingDict.Add(vcr.vcrID, recording);
        }

    }

    bool inRandPlaybackMode = false;

    void ConfigAllVCRParent (InputVCRMode vcrMode)
    {
        foreach(InputVCR vcr in allVCR)
        {
            SetVCRParent(vcr, vcrMode);
        }
    }

    public void SetAutoRandomPlayback(bool enable)
    {
        inRandPlaybackMode = enable;
        if (enable)
        {
            ConfigAllVCRParent(InputVCRMode.Playback);
            PlayNextRecording();
        }
        else
        {
            ConfigAllVCRParent(InputVCRMode.Passthru);
            DoToAllVCR(VCRAction.Stop);
        }
    }

    void PlaybackEnd()
    {
        if(inRandPlaybackMode)
        {
            Invoke("PlayNextRecording", 5);
        }

        lastPlayRecordName = nextPlayRecordName;
    }


    string nextPlayRecordName = "";
    string lastPlayRecordName = "";
    bool nextPlayDecided = false;

    void SetNextPlayRecord(string fileName)
    {
        if(fileName != null)
        {
            nextPlayRecordName = fileName;
            LoadRecord(nextPlayRecordName);
            nextPlayDecided = true;
        }
    }

    List<string> allRecordingFileNames = new List<string>();
    List<string> GetAllRecordings()
    {
        allRecordingFileNames.Clear();
        string[] allFileNames = Directory.GetFiles(recordingsPath);
        foreach (string fileName in allRecordingFileNames)
        {
            if(Path.GetExtension(fileName) == fileExtensionName)
            {
                allRecordingFileNames.Add(fileName);
            }
        }
        
        return allRecordingFileNames;
    }
    
    void PlayNextRecording()
    {
        nextPlayDecided = false;
        string tempBuf = null;

        var myFiles = GetAllRecordings();
        for (int i = 0;i < 5;i++)
        {
            int randIndex = UnityEngine.Random.Range(0, myFiles.Count);
            tempBuf = myFiles[randIndex];
            if(tempBuf != lastPlayRecordName)
            {
                SetNextPlayRecord(tempBuf);
            }
            
        }
        
        if(!nextPlayDecided)
            SetNextPlayRecord(tempBuf);

        if(nextPlayDecided)
            DoToAllVCR(VCRAction.Play);
        else
        {
            Debug.LogWarning("failed to play next recording");
        }

        return;
        
    }

    void SetVCRParent(InputVCR vcr, InputVCRMode vcrMode)
    {
        if(vcrMode == InputVCRMode.Playback)
        {
            vcr.transform.parent = vcrPlaybackModeParent;
        }
        else
        {
            vcr.transform.parent = vcr.originalParent;
        }
    }
}
