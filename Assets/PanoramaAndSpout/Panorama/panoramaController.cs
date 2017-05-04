using UnityEngine;
using System.Collections;

public class panoramaController : MonoBehaviour {

    public int totalNumCam; //please make sure this num can be completely divide 360 

    public Transform camsRoot;
    public Camera cam;
    public FromSameView fromSameView;

    Camera[] allCams;
    float widthProportion;
    
    // Use this for initialization
    void Start () {
        int targetWidth = 0, targetHeight = 0;
        if (cam != null && totalNumCam >= 1)
        {
            if(cam.targetTexture != null)
            {
                RenderTexture renderTex = cam.targetTexture;
                targetWidth = renderTex.width;
                targetHeight = renderTex.height;
            }
            else
            {
                targetWidth = Screen.width;
                targetHeight = Screen.height;
            }

            cam.aspect = ((float)targetWidth / totalNumCam) / targetHeight;
            
            widthProportion = 1.0f / totalNumCam;
            cam.rect = new Rect(0,0, widthProportion, 1);
            GameObject camGO = cam.gameObject;

            float hFOVDeg = 360.0f / totalNumCam;
            if(hFOVDeg > 90)
            {
                hFOVDeg = 90;
            }

            setVFOV(cam, hFOVDeg);

            AudioListener audioListener = cam.GetComponent<AudioListener>();
            if (audioListener != null)
                Destroy(audioListener);

            
            if (fromSameView != null)
                fromSameView.enabled = false;

            allCams = new Camera[totalNumCam];
            allCams[0] = cam;
            
            for (int i = 1;i < totalNumCam;i++)
            {
                GameObject duplicatedObj = Object.Instantiate<GameObject>(camGO);
                allCams[i] = duplicatedObj.GetComponent<Camera>();
            }

            for(int i = 1;i < totalNumCam;i++)
            {
                allCams[i].transform.parent = camsRoot;
            }

            if (fromSameView != null)
                fromSameView.enabled = true;
            
            UpdateCams();

        }

	}

    float getHFOVDeg(Camera camera)
    {
        float vFOVRad = camera.fieldOfView * Mathf.Deg2Rad;
        float hFOVDeg = 2 * Mathf.Rad2Deg * Mathf.Atan(camera.aspect * Mathf.Tan(vFOVRad / 2));
        return hFOVDeg;
    }

    void setVFOV(Camera camera, float HFOVDeg)
    {
        float suggestVFOVDeg = 2 * Mathf.Rad2Deg * Mathf.Atan(Mathf.Tan(HFOVDeg / 2.0f * Mathf.Deg2Rad) / camera.aspect);
        camera.fieldOfView = suggestVFOVDeg; 
    }
	
	// Update is called once per frame
	void Update () {
        UpdateCams();
    }

    void UpdateCams()
    {
        if (cam != null && totalNumCam >= 1)
        {
            float hFOVDeg = getHFOVDeg(cam);
            for (int i = 1; i < totalNumCam; i++)
            {
                Camera duplicatedCam = allCams[i];
                duplicatedCam.rect = new Rect(i * widthProportion, 0, widthProportion, 1);
                Vector3 angles = duplicatedCam.transform.localEulerAngles;
                angles.y = i * hFOVDeg;
                duplicatedCam.transform.localEulerAngles = angles;
            }
        }
    }
}
