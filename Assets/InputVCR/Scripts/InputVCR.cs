/* InputVCR.cs
 * Copyright Eddie Cameron 2012 (See readme for licence)
 * ----------
 * Place on any object you wish to use to record or playback any inputs for
 * Switch modes to change current behaviour
 *   - Passthru : object will use live input commands from player
 *   - Record : object will use, as well as record, live input commands from player
 *   - Playback : object will use either provided input string or last recorded string rather than live input
 *   - Pause : object will take no input (buttons/axis will be frozen in last positions)
 * 
 * -----------
 * Recordings are all saved to the 'currentRecording' member, which you can get with GetRecording(). This can then be copied 
 * to a new Recording object to be saved and played back later.
 * Call ToString() on these recordings to get a text version of this if you want to save a recording after the program exits.
 * -----------
 * To use, place in a gameobject, and have all scripts in the object refer to it instead of Input.
 * 
 * eg: instead of Input.GetButton( "Jump" ), you would use vcr.GetButton( "Jump" ), where vcr is a 
 * reference to the component in that object
 * If VCR is in playback mode, and the "Jump" input was recorded, it will give the recorded input state, 
 * otherwise it will just pass through the live input state
 * 
 * Note, InputVCR can't be statically referenced like Input, since you may have multiple objects playing
 * different recordings, or an object playing back while another is taking live input...
 * ----------
 * Use this snippet in scripts you wish to replace Input with InputVCR, so they can be used in objects without a VCR as well:
 
  private bool useVCR;
  private InputVCR vcr;
  
  void Awake()
  {
    Transform root = transform;
	while ( root.parent != null )
		root = root.parent;
	vcr = root.GetComponent<InputVCR>();
	useVCR = vcr != null;
  }
  
 * Then Replace any input lines with:
  
  if ( useVCR )
  	<some input value> = vcr.GetSomeInput( "someInputName" );
  else
  	<some input value> = Input.GetSomeInput( "someInputName" );
  
 * Easy! 
 * -------------
 * More information and tools at grapefruitgames.com, @eddiecameron, or support@grapefruitgames.com
 * 
 * This script is open source under the GNU LGPL licence. Do what you will with it! 
 * http://www.gnu.org/licenses/lgpl.txt
 * 
 */ 

 //add VRTK support by KTChou at 2017/5/2

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Text;
using System.IO;
using System.Collections.Generic;
using VRTK;

public class InputVCR : MonoBehaviour 
{

    #region Inspector properties
    public string vcrID;
    public InputInfo[] inputsToRecord;  // list of axis and button names ( from Input manager) that should be recorded
	
	public bool recordMouseEvents;		// whether mouse position/button states should be recorded each frame (mouse axes are separate from this)
	
	public bool syncRecordLocations = true;	// whether position/rotation info is stored automatically
	public float autoSyncLocationRate = 1f;
	public bool snapToSyncedLocation = true;	// whether this transform will snap to recorded locations, or left accessible for your own handling
	
	public int recordingFrameRate = 60;
	
	[SerializeField]
	private InputVCRMode _mode = InputVCRMode.Passthru; // initial mode that vcr is operating in
	public InputVCRMode mode
	{
		get { return _mode; }
	}

    public bool recordLocalInfo; //local rotation, local position
    
    #endregion
	
	float nextPosSyncTime = -1f;
	float realRecordingTime;
	[HideInInspector]
	public Recording currentRecording;		// the recording currently in the VCR. Copy or ToString() this to save.
	public float currentTime{
		get {
			return currentFrame / (float)currentFrameRate; }
	}
	public int currentFrameRate{
		get {
			if ( currentRecording == null )
				return recordingFrameRate;
			else
				return currentRecording.frameRate;
		}
	}
	public int currentFrame{ get; private set; }	// current frame of recording/playback
	
	Queue<FrameProperty> nextPropertiesToRecord = new Queue<FrameProperty>();	// if SyncLocation or SyncProperty are called, this will hold their results until the recordstring is next written to
		
	Dictionary<string, InputInfo> lastFrameInputs = new Dictionary<string, InputInfo>();	// list of inputs from last frame (for seeing what buttons have changed state)
	Dictionary<string, InputInfo> thisFrameInputs = new Dictionary<string, InputInfo>();
		
	float playbackTime;
	
	public event System.Action finishedPlayback;    // sent when playback finishes

    //add VRTK support
    [System.Serializable]
    public class VRCtrlerInfo
    {
        public LeftRightVar var;
        public VRCtrlerInput input;
    }

    public enum VRCtrlerInput
    {
        TouchpadButton,
        TouchpadAxis,
        TriggerButton,
        TriggerAxis,
        GrabButton
    }

    public VRCtrlerInfo[] recordVRInputs;
    public bool recordTeleportEvent;
    public VRTK_BasicTeleport basicTeleport;
    public VRTK_ControllerEvents leftCtrlerEventInfo;
    public VRTK_ControllerEvents rightCtrlerEventInfo;
    VRTK_ControllerEvents[] ctrlerEventInfos = new VRTK_ControllerEvents[2];
    string[] vrTouchpadNames = new string[] { "vrLeftTouchpad", "vrRightTouchpad" };
    string[] vrTriggerNames = new string[] { "vrLeftTrigger", "vrRightTrigger" };
    string[] vrGrabNames = new string[] { "vrLeftGrab", "vrRightGrab" };

    [HideInInspector]
    public Transform originalParent;
    
    public Vector3 initLocalPos;
    public Quaternion initLocalRot;

    //add some event-driven mechanism
    UnityEvent PlaybackStart = new UnityEvent(); //instead of continuing from pause state, it start playing a recording from certain timestamp.
    UnityEvent PlaybackStop = new UnityEvent(); //a recording stopped(either stopped by someone and it reach the end)
    UnityEvent NewRecordingStart = new UnityEvent();
    UnityEvent RecordingStop = new UnityEvent();

    //feel free to add other kinds of event

    public void OnTeleported(object sender, DestinationMarkerEventArgs destMarkerEventInfo)
    {
        SyncProperty("teleported", "1");
    }

    private void Awake()
    {
        originalParent = transform.parent;
        initLocalPos = transform.localPosition;
        initLocalRot = transform.localRotation;
        
        if (basicTeleport == null)
        {
            var GO = GameObject.FindGameObjectWithTag("VRTKTeleport");
            if(GO)
                basicTeleport = GO.GetComponent<VRTK_BasicTeleport>();
        }
        
        if(basicTeleport != null && recordTeleportEvent)
            basicTeleport.Teleported += OnTeleported;
        
        if (leftCtrlerEventInfo == null)
        {
            var GO = GameObject.FindGameObjectWithTag("VRTKLeftCtrler");
            if (GO)
                leftCtrlerEventInfo = GO.GetComponent<VRTK_ControllerEvents>();
        }
        
        if (rightCtrlerEventInfo == null)
        {
            var GO = GameObject.FindGameObjectWithTag("VRTKRightCtrler");
            if (GO)
                rightCtrlerEventInfo = GO.GetComponent<VRTK_ControllerEvents>();
        }
        
        ctrlerEventInfos[0] = leftCtrlerEventInfo;
        ctrlerEventInfos[1] = rightCtrlerEventInfo;
    }


    /// <summary>
    /// Start recording. Will append to already started recording
    /// </summary>
    public void Record()
	{
		if ( currentRecording == null || currentRecording.recordingLength == 0 )
			NewRecording();
		else
			_mode = InputVCRMode.Record;
	}
	
	/// <summary>
	/// Starts a new recording. If old recording wasn't saved it will be lost forever!
	/// </summary>
	public void NewRecording()
	{
		// start recording live input
		currentRecording = new Recording( recordingFrameRate );
		currentFrame = 0;
		realRecordingTime = 0;
		
		nextPosSyncTime = -1f;
		nextPropertiesToRecord.Clear ();
		
		_mode = InputVCRMode.Record;

        NewRecordingStart.Invoke();

    }
	
	/// <summary>
	/// Start playing back the current recording, if present.
	/// If currently paused, will just resume
	/// </summary>
	public void Play()
	{
		// if currently paused during playback, will continue
		if ( mode == InputVCRMode.Pause )
			_mode = InputVCRMode.Playback;
		else
		{
			// if not given any input string, will use last recording
			Play ( currentRecording );
		}
	}
	
	/// <summary>
	/// Play the specified recording, from optional specified time
	/// </summary>
	/// <param name='recording'>
	/// Recording.
	/// </param>
	/// <param name='startRecordingFromTime'>
	/// OPTIONAL: Time to start recording at
	/// </param>
	public void Play( Recording recording, float startRecordingFromTime = 0 )
	{	
		currentRecording = new Recording( recording );
		currentFrame = recording.GetClosestFrame ( startRecordingFromTime );
		
        thisFrameInputs.Clear();
        lastFrameInputs.Clear();
        
        _mode = InputVCRMode.Playback;
		playbackTime = startRecordingFromTime;

        PlaybackStart.Invoke();
	}
	
	/// <summary>
	/// Pause recording or playback. All input will be blocked while paused
	/// </summary>
	public void Pause()
	{
		_mode = InputVCRMode.Pause;
	}
	
	/// <summary>
	/// Stop recording or playback and rewind Live input will be passed through
	/// </summary>
	public void Stop()
	{			
        if(_mode == InputVCRMode.Playback)
        {
            PlaybackStop.Invoke();
        }
        else if(_mode == InputVCRMode.Record)
        {
            RecordingStop.Invoke();
        }

		_mode = InputVCRMode.Passthru;
		currentFrame = 0;
		playbackTime = 0;
	}
	
	/// <summary>
	/// Records the location/rotation of this object during a recording, so when it is played back, object is sure to be here.
	/// Use this if you have drift(and don't want it) in your recordings due to physics/other external inputs.
	/// </summary>
	public void SyncPositionAndRotation()
	{
		if ( mode != InputVCRMode.Record )
		{
			Debug.LogWarning ( "Tried to record location, but VCR isn't recording" );
			return;
		}
        
		SyncProperty( "position", Vector3ToString ( recordLocalInfo? transform.localPosition : transform.position ) );
		SyncProperty( "rotation", Vector3ToString ( recordLocalInfo? transform.localEulerAngles : transform.eulerAngles ) );
	}
	
	/// <summary>
	/// Adds a custom property to the recording, so you can sync other (non-input) events as well.
	/// eg: doors opening, enemy spawning, etc 
	/// </summary>
	/// <param name='propertyName'>
	/// Property name.
	/// </param>
	/// <param name='propertyValue'>
	/// Property value.
	/// </param>
	public void SyncProperty( string propertyName, string propertyValue )
	{
		// duplicates dealt with when recorded
		FrameProperty frameProp = new FrameProperty( propertyName, propertyValue );
		if ( !nextPropertiesToRecord.Contains ( frameProp ) )
			nextPropertiesToRecord.Enqueue ( frameProp );
	}
	
	/// <summary>
	/// Gets a copy of the current recording
	/// </summary>
	/// <returns>
	/// The recording.
	/// </returns>
	public Recording GetRecording()
	{
		return new Recording( currentRecording );
	}

    Dictionary<string, InputInfo> changedInputs = new Dictionary<string, InputInfo>();

    void LateUpdate()
	{
        //print("late update in vcr");

		if ( _mode == InputVCRMode.Playback )
		{
			// update last frame and this frame
			// this way, all changes are transmitted, even if a button press lasts less than a frame (like in Input)
			lastFrameInputs = thisFrameInputs;
			
			int lastFrame = currentFrame;
			currentFrame = currentRecording.GetClosestFrame ( playbackTime );
			
			if ( currentFrame > currentRecording.totalFrames )
			{
				// end of recording
				if ( finishedPlayback != null )
					finishedPlayback( );
				Stop ();
			}
			else
			{
                changedInputs.Clear();
                // go through all changes in recorded input since last frame
                for ( int frame = lastFrame + 1; frame <= currentFrame; frame++ )
				{
                    if (inputsToRecord.Length > 0)
                        foreach ( InputInfo input in currentRecording.GetInputs ( frame ) )
					    {
						    // thisFrameInputs only updated once per game frame, so all changes, no matter how brief, will be marked
						    // if button has changed
						    if ( !thisFrameInputs.ContainsKey ( input.inputName ) || !thisFrameInputs[input.inputName].Equals( input ) )
						    {
							    if ( changedInputs.ContainsKey ( input.inputName ) )
								    changedInputs[input.inputName] = input;
							    else
								    changedInputs.Add( input.inputName, input );
						    }
					    }
				
                    //no interpolation
					if ( snapToSyncedLocation )	// custom code more effective, but this is enough sometimes
					{
                        Vector3 resultPos = recordLocalInfo ? transform.localPosition : transform.position;
                        Vector3 resultAngles = recordLocalInfo ? transform.localEulerAngles : transform.eulerAngles;
                        string posString = currentRecording.GetProperty ( frame, "position" );
						if ( !string.IsNullOrEmpty ( posString ) )
							resultPos = ParseVector3 ( posString );
						
						string rotString = currentRecording.GetProperty ( frame, "rotation" );
						if ( !string.IsNullOrEmpty( rotString ) )
							resultAngles = ParseVector3 ( rotString );
                        SetPosAndRot(resultPos, resultAngles);

                    }
				}
				
				// update input to be used tihs frame
				foreach( KeyValuePair<string, InputInfo> changedInput in changedInputs )
				{
					if ( thisFrameInputs.ContainsKey ( changedInput.Key ) )
						thisFrameInputs[changedInput.Key] = changedInput.Value;
					else
						thisFrameInputs.Add ( changedInput.Key, changedInput.Value );
				}
				
				playbackTime += Time.deltaTime;
			}
		}
		else if ( _mode == InputVCRMode.Record )
		{	
			realRecordingTime += Time.deltaTime;
			// record current input to frames, until recording catches up with realtime
			while ( currentTime < realRecordingTime )
			{
				// mouse position & buttons if required
				if ( recordMouseEvents )
				{
					currentRecording.AddProperty( currentFrame, new FrameProperty( "mousePos", Input.mousePosition.x.ToString() + "," + Input.mousePosition.y ) );
					
					for( int i = 0; i < 3; i++ )
					{
						InputInfo mouseInput = new InputInfo();
						mouseInput.inputName = "mousebutton" + i;
						mouseInput.isAxis = false;
						mouseInput.mouseButtonNum = i;
						currentRecording.AddInput ( currentFrame, mouseInput );
					}
				}
				
				// and buttons
				foreach( InputInfo input in inputsToRecord )
				{
					if ( input.isAxis )
						input.axisValue = Input.GetAxis ( input.inputName );
					else if ( input.mouseButtonNum >= 0 )	// mouse buttons recorded above 
						input.buttonState = Input.GetButton ( input.inputName );
					currentRecording.AddInput ( currentFrame, input );
				}

                //KT edited: sync vr properties
                foreach(VRCtrlerInfo ctrlerInfo in recordVRInputs)
                {
                    InputInfo vrInput = new InputInfo();
                    int varVal = (int)ctrlerInfo.var; //left or right

                    if (varVal < 2)
                    {
                        if (ctrlerInfo.input == VRCtrlerInput.TriggerAxis)
                        {
                            vrInput.inputName = vrTriggerNames[varVal] + "Axis";
                            vrInput.axisValue = ctrlerEventInfos[varVal].GetTriggerAxis();
                            currentRecording.AddInput(currentFrame, vrInput);
                        }
                        else if (ctrlerInfo.input == VRCtrlerInput.TouchpadButton)
                        {
                            vrInput.inputName = vrTouchpadNames[varVal] + "Pressed";
                            vrInput.buttonState = ctrlerEventInfos[varVal].touchpadPressed;
                            currentRecording.AddInput(currentFrame, vrInput);
                        }
                    }
                }
                
                // synced location
                if ( syncRecordLocations && Time.time > nextPosSyncTime )
				{
                    //KT edited: sync local info or world info
					SyncPositionAndRotation ();	// add position to properties
					nextPosSyncTime = Time.time + 1f / autoSyncLocationRate;
				}
				
				// and any other properties
				foreach( FrameProperty prop in nextPropertiesToRecord )
					currentRecording.AddProperty ( currentFrame, prop );
				nextPropertiesToRecord.Clear ();
				
				currentFrame++;
			}
		}
	}

    void SetPosAndRot(Vector3 pos, Vector3 eulerAngles)
    {
        if (recordLocalInfo)
        {
            transform.localPosition = pos;
            transform.localEulerAngles = eulerAngles;
        }
        else
        {
            transform.position = pos;
            transform.eulerAngles = eulerAngles;
        }
    }



    // These methods replace those in Input, so that this object can ignore whether it is record
    #region Input replacements
    public bool GetButton( string buttonName )
	{
		if ( _mode == InputVCRMode.Pause )
			return false;
		
		if ( _mode == InputVCRMode.Playback && thisFrameInputs.ContainsKey ( buttonName ) )
			return thisFrameInputs[buttonName].buttonState;
		else
			return Input.GetButton ( buttonName );
	}
	
	public bool GetButtonDown( string buttonName )
	{
		if ( _mode == InputVCRMode.Pause )
			return false;
		
		if ( _mode == InputVCRMode.Playback && thisFrameInputs.ContainsKey( buttonName ) )
			return ( thisFrameInputs[buttonName].buttonState && ( lastFrameInputs == null || !lastFrameInputs.ContainsKey ( buttonName ) || !lastFrameInputs[buttonName].buttonState ) );
		else
			return Input.GetButtonDown ( buttonName );
	}
	
	public bool GetButtonUp( string buttonName )
	{
		if ( _mode == InputVCRMode.Pause )
			return false;
		
		if ( _mode == InputVCRMode.Playback && thisFrameInputs.ContainsKey( buttonName ) )
			return ( !thisFrameInputs[buttonName].buttonState && ( lastFrameInputs == null || !lastFrameInputs.ContainsKey ( buttonName ) || lastFrameInputs[buttonName].buttonState ) );
		else
			return Input.GetButtonUp ( buttonName );
	}
	
	public float GetAxis( string axisName )
	{
		if ( _mode == InputVCRMode.Pause )
			return 0;
		
		if ( _mode == InputVCRMode.Playback && thisFrameInputs.ContainsKey( axisName ) )
			return thisFrameInputs[axisName].axisValue;
		else
			return Input.GetAxis ( axisName );
	}
	
	public bool GetMouseButton( int buttonNum )
	{
		if ( _mode == InputVCRMode.Pause )
			return false;
		
		string mouseButtonName =  "mousebutton" + buttonNum.ToString();
		if ( _mode == InputVCRMode.Playback && thisFrameInputs.ContainsKey ( mouseButtonName ) )
			return thisFrameInputs[mouseButtonName].buttonState;
		else
			return Input.GetMouseButton( buttonNum );
	}
	
	public bool GetMouseButtonDown( int buttonNum )
	{
		if ( _mode == InputVCRMode.Pause )
			return false;
		
		string mouseButtonName =  "mousebutton" + buttonNum.ToString();
		if ( _mode == InputVCRMode.Playback && thisFrameInputs.ContainsKey ( mouseButtonName ) )
			return ( thisFrameInputs[ mouseButtonName ].buttonState && ( lastFrameInputs == null || !lastFrameInputs.ContainsKey ( mouseButtonName ) || !lastFrameInputs[mouseButtonName].buttonState ) );
		else
			return Input.GetMouseButtonDown( buttonNum );
	}
	
	public bool GetMouseButtonUp( int buttonNum )
	{
		if ( _mode == InputVCRMode.Pause )
			return false;
		
		string mouseButtonName =  "mousebutton" + buttonNum.ToString();
		if ( _mode == InputVCRMode.Playback && thisFrameInputs.ContainsKey ( "mousebutton" + buttonNum.ToString() ) )
			return ( !thisFrameInputs[ mouseButtonName ].buttonState && ( lastFrameInputs == null || !lastFrameInputs.ContainsKey ( mouseButtonName ) || lastFrameInputs[mouseButtonName].buttonState ) );
		else
			return Input.GetMouseButtonUp( buttonNum );
	}
	
    public enum LeftRightVar
    {
        Left = 0,
        Right = 1,
        None = 100
    }

    public bool GetVRTouchpadPressed(LeftRightVar var)
    {
        string buttonName = vrTouchpadNames[(int)var] + "Pressed";
        
        if (_mode == InputVCRMode.Pause)
            return false;
        else if (_mode == InputVCRMode.Playback && thisFrameInputs.ContainsKey(buttonName))
            return thisFrameInputs[buttonName].buttonState;
        else
            return ctrlerEventInfos[(int)var].touchpadPressed;
        
    }


    public float GetTriggerAxis(LeftRightVar var)
    {
        string axisName = vrTriggerNames[(int)var] + "Axis";
        if (_mode == InputVCRMode.Pause)
            return 0;
        else if (_mode == InputVCRMode.Playback && thisFrameInputs.ContainsKey(axisName))
            return thisFrameInputs[axisName].axisValue;
        else
            return ctrlerEventInfos[(int)var].GetTriggerAxis();
    }


    public Vector3 mousePosition
	{	
		get {
			if ( _mode == InputVCRMode.Pause )
				return Vector3.zero;
			
			if ( _mode == InputVCRMode.Playback )
			{
				string mousePos = currentRecording.GetProperty ( currentFrame, "mousepos" );
				if ( !string.IsNullOrEmpty ( mousePos ) )
				{
					string[] splitPos = mousePos.Split( ",".ToCharArray() );
					if ( splitPos.Length == 2 )
					{
						float x,y;
						if ( float.TryParse ( splitPos[0], out x ) && float.TryParse ( splitPos[1], out y ) )
							return new Vector3( x, y, 0 );
					}
				}
			}
			
			return Input.mousePosition;
		}
	}
	
	public string GetProperty( string propertyName )
	{
		return currentRecording.GetProperty ( currentFrame, propertyName );
	}
	#endregion
	
	public static string Vector3ToString( Vector3 vec )
	{
		return vec.x.ToString () + "," + vec.y + "," + vec.z;
	}
	
	public static Vector3 ParseVector3( string vectorString )
	{
		string[] splitVecString = vectorString.Split( ",".ToCharArray () );
		float x,y,z;
		if( splitVecString.Length == 3 && float.TryParse ( splitVecString[0], out x ) && float.TryParse ( splitVecString[1], out y ) && float.TryParse ( splitVecString[2], out z ) )
			return new UnityEngine.Vector3( x, y, z );
		
		return Vector3.zero;
	}
}
