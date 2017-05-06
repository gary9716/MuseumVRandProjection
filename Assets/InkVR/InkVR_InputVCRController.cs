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
    
    public string defaultRecordName;
    public bool enableKeyCtrlFunc;
    public bool enableRecordFunc;
    public float nextVideoInterval = 3;

    [HideInInspector]
    public bool chooseCurrentRecordingFirst;

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
        if (chooseCurrentRecordingFirst)
        {
            recordingChosenOrder[0] = vcr.currentRecording;
            recordingChosenOrder[1] = recordingDict[vcr.vcrID];
        }
        else
        {
            recordingChosenOrder[1] = vcr.currentRecording;
            recordingChosenOrder[0] = recordingDict[vcr.vcrID];
        }

        recording = recordingChosenOrder[0];
        if (recording != null)
        {
            vcr.Play(recording, 0);
        }
        else
        {
            recording = recordingChosenOrder[1];
            //print("make vcr secondly:" + vcr.vcrID + " play content:" + recording.ToString());
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
        if (allVCR != null && allVCR.Count > 0 && allVCR[0] != null)
            return allVCR[0].mode;
        else
            return InputVCRMode.Passthru;
    }
    
    private void Awake()
    {
        if (allVCR.Count == 0)
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
            print("vcr GO name:" + vcr.gameObject.name + ",id:" + vcr.vcrID);
            vcrDict.Add(vcr.vcrID, vcr);
        }
        
        vcrEventNotifier = allVCR[0];
        vcrEventNotifier.finishedPlayback += PlaybackEnd;
        
        DoToAllVCR(VCRAction.Stop);
    }
    
    public bool allVCRAreHoldingRecording()
    {
        foreach(InputVCR vcr in allVCR)
        {
            if (vcr.currentRecording == null)
            {
                Debug.LogWarning("There is one vcr having no recording feature");
                return false;
            }
                
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
            writer.Write(vcr.currentRecording.ToString());
            //print("record id:" + vcr.vcrID + ",val:" + vcr.GetRecording().ToString());
        }
        writer.WriteObjectEnd();
        string content = sb.ToString();
        print("record saved, name:" + fileName + ",content:" + content);
        File.WriteAllText(GetFullFilePath(fileName), content);
        
    }

    public void LoadRecord(string fileName, bool alreadyFullPath)
    {
        var fileNameFullPath = alreadyFullPath ? fileName : GetFullFilePath(fileName);
        string jsonStr = File.ReadAllText(fileNameFullPath);
        JsonData data = JsonMapper.ToObject(jsonStr);
        recordingDict.Clear();
        foreach (InputVCR vcr in allVCR)
        {
            string recordingJsonStr = (string)data[vcr.vcrID];
            Recording recording = Recording.ParseRecording(recordingJsonStr);
            if (recording == null)
                print("parsed failed");
            else
                print("parsed id:" + vcr.vcrID + ",result:" + recording.ToString());
            recordingDict.Add(vcr.vcrID, recording);
        }

        print("record loaded, name:" + fileName);
    }
    
    bool inRandPlaybackMode = false;

    public void SetAutoRandomPlayback(bool enable)
    {
        inRandPlaybackMode = enable;
        CancelInvoke("PlayNextRecording");
        if (enable)
        {
            PlayNextRecording();
        }
        else
        {
            DoToAllVCR(VCRAction.Stop);
        }
    }

    public void PlaybackEnd()
    {
        if(inRandPlaybackMode)
        {
            Invoke("PlayNextRecording", nextVideoInterval);
        }

        lastPlayRecordName = nextPlayRecordName;

        print(Path.GetFileName(lastPlayRecordName) + " playback ended");
        
    }
    
    string nextPlayRecordName = "";
    string lastPlayRecordName = "";
    bool nextPlayDecided = false;

    void SetNextPlayRecord(string fileName)
    {
        if(fileName != null)
        {
            nextPlayRecordName = fileName;
            print("vcr prepare playing:" + Path.GetFileName(nextPlayRecordName));
            LoadRecord(nextPlayRecordName, true);
            nextPlayDecided = true;
        }
    }

    List<string> allRecordingFileNames = new List<string>();
    List<string> GetAllRecordings()
    {
        //refresh recording list from file system
        allRecordingFileNames.Clear();
        string[] allFileNames = Directory.GetFiles(recordingsPath);
        foreach (string fileName in allFileNames)
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
        for (int i = 0;i < 2;i++)
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
            Debug.LogWarning("failed to play next recording");
        
        return;
        
    }
    
    void ConfigAllVCRParent(InputVCRMode vcrMode)
    {
        foreach (InputVCR vcr in allVCR)
        {
            SetVCRParent(vcr, vcrMode);
        }
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
    
    AppMode currentAppMode = AppMode.Unknown;

    public void SwitchAppMode()
    {
        if(currentAppMode == AppMode.VR)
        {
            SetupForAppMode(AppMode.Video);
            SetAutoRandomPlayback(true);
        }
        else
        {
            SetupForAppMode(AppMode.VR);
            SetAutoRandomPlayback(false);
        }
    }

    void RestoreAllVCRPosAndRot()
    {
        foreach(InputVCR vcr in allVCR)
        {
            vcr.transform.localPosition = vcr.initLocalPos;
            vcr.transform.localRotation = vcr.initLocalRot;
        }
    }

    public void SetupForAppMode(AppMode appMode)
    {
        DoToAllVCR(VCRAction.Stop);
        if(appMode == AppMode.VR)
        {
            ConfigAllVCRParent(InputVCRMode.Passthru);
            RestoreAllVCRPosAndRot();
        }
        else if(appMode == AppMode.Video)
        {
            ConfigAllVCRParent(InputVCRMode.Playback);
        }

        currentAppMode = appMode;
        uiManager.SetAppModeTxt(appMode);
    }


    void Update()
    {
        if (enableKeyCtrlFunc)
        {
            if (enableRecordFunc && Input.GetKeyDown(KeyCode.R))
                ToggleRecording();
            else if (Input.GetKeyDown(KeyCode.S))
                SwitchAppMode();
            else if (Input.GetKeyDown(KeyCode.O))
                outputAllRecordInfo();
            else
            {
                for (int i = 0; i < keyCodes.Length; i++)
                {
                    if (Input.GetKeyDown(keyCodes[i]))
                    {
                        int numberPressed = i;
                        //Debug.Log(numberPressed);
                        if(LoadRecordWithIndexInDir(numberPressed))
                        {
                            if(currentAppMode != AppMode.Video)
                            {
                                SetupForAppMode(AppMode.Video);
                            }

                            TogglePlaying();
                        }
                        
                    }

                }
            }
        }

    }
    
    public bool LoadRecordWithIndexInDir(int index)
    {
        List<string> allRecord = GetAllRecordings();
        if (index >= 0 && index < allRecord.Count)
        {
            LoadRecord(allRecord[index], true);
            return true;
        }
        else
        {
            Debug.Log("index out of bound, cannot load this record");
            return false;
        }

    }

    public void outputAllRecordInfo()
    {
        List<string> allRecord = GetAllRecordings();
        for (int i = 0; i < allRecord.Count; i++)
        {
            print("i:" + i + ",name:" + allRecord[i]);
        }

    }
    
    private KeyCode[] keyCodes = {
         KeyCode.Alpha0,
         KeyCode.Alpha1,
         KeyCode.Alpha2,
         KeyCode.Alpha3,
         KeyCode.Alpha4,
         KeyCode.Alpha5,
         KeyCode.Alpha6,
         KeyCode.Alpha7,
         KeyCode.Alpha8,
         KeyCode.Alpha9,
     };

}
