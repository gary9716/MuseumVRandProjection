using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using LitJson;

public class InkVR_InputVCRController : MonoBehaviour {

    public string recordFolderName;
    public List<InputVCR> allVCR;
    Dictionary<string, InputVCR> vcrDict;
    Dictionary<string, Recording> recordingDict;
    string recordingsPath;

    private bool isRecording;
    private bool isPlaying;

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
        else if(action == VCRAction.Record)
        {
            isRecording = true;
        }
        else if(action == VCRAction.Pause)
        {
            isPlaying = false;
        }
    }


    public void StartPlaying()
    {
        if (isPlaying)
        {
            // pause
            DoToAllVCR(VCRAction.Pause);
        }
        else
        {
            // unpause
            DoToAllVCR(VCRAction.Play);
        }
    }


    public void StartRecording()
    {
        if (isRecording)
            DoToAllVCR(VCRAction.Stop);
        else
        {
            DoToAllVCR(VCRAction.NewRecording);
        }
        
    }

    private void Awake()
    {
        if(string.IsNullOrEmpty(recordFolderName))
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
    }

    /*
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            StartPlay();
    }
    */


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
   
}
