using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInOutEffect : MonoBehaviour {

    public Texture2D blackTex;
    public int drawDepth = -1000;

    [Range(0,1)]
    public float alpha = 1;

    public enum FadeDir
    {
        Out = 1,
        In = -1,
        None = 0
    }
    
    public FadeDir fadeDir = FadeDir.None;
    public float fadeSpeed = 0.2f;
    public Camera cam;
    public Rect rect;

    private void Start()
    {
        cam = GetComponent<Camera>();
        rect = new Rect(0, 0, 1920, 1200);    
    }

    private void OnGUI()
    {
        alpha += ((int)fadeDir) * fadeSpeed * Time.deltaTime;
        alpha = Mathf.Clamp01(alpha);
        Color originalColor = GUI.color;
        originalColor.a = alpha;

        GUI.color = originalColor;
        GUI.depth = drawDepth;
        GUI.DrawTexture(rect, blackTex);

    }


}
