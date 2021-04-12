using System;
using System.Collections.Generic;
using UnityEngine;

public class UserListener : MonoBehaviour
{
    private GestureRecognition gr = new GestureRecognition();
    private GameObject activeController = null;
    private double similarity = 0;
    private Vector3 pos = Vector3.zero;
    private double scale = 0;
    private Vector3 dir0 = Vector3.zero;
    private Vector3 dir1 = Vector3.zero;
    private Vector3 dir2 = Vector3.zero;
    private List<InteractionMachine.Interaction> subs = new List<InteractionMachine.Interaction>();
    private double elapsed = 0;

    [Header("Parameters")]
    /// <summary>
    /// The frequency at which the interactions will be notified with sensorial data in seconds.
    /// </summary>
    [Tooltip("The frequency at which the sensorial data will be sent.")]
    public double frequency = 2;

    /// <summary>
    /// The path to the Gestures.dat file containing the recorded gestures data.
    /// </summary>
    [Tooltip("The path to the Gestures.dat file containing the recorded gestures data (Editor).")]
    public string gesturesFilePath;

    /// <summary>
    /// The file from which to load gestures on startup.
    /// </summary>
    [Tooltip("The file from which to load gestures on startup.")]
    public string gesturesFileName;

    [Header("Components")]
    /// <summary>
    /// The left hand game object.
    /// </summary>
    [Tooltip("The left hand game object.")]
    public GameObject leftHand;

    /// <summary>
    /// The right hand game object.
    /// </summary>
    [Tooltip("The right hand game object.")]
    public GameObject rightHand;

    /// <summary>
    /// Adds an interaction to the list of subscribers of the user actions.
    /// </summary>
    /// <param name="interaction">The interaction to be added.</param>
    public void Subscribe(InteractionMachine.Interaction interaction)
    {
        subs.Add(interaction);
    }

    /// <summary>
    /// Removes an interaction from the list of subscribers of the user actions.
    /// </summary>
    /// <param name="interaction">The interaction to be added.</param>
    public void Unsubscribe(InteractionMachine.Interaction interaction)
    {
        subs.Remove(interaction);
    }

    // Start is called before the first frame update
    void Start()
    {

        // Find the location for the gesture database (.dat) file
#if UNITY_EDITOR
        // When running the scene inside the Unity editor,
        // we can just load the file from the Assets/ folder:
        string _gesturesFilePath = gesturesFilePath;
#elif UNITY_ANDROID
        // On android, the file is in the .apk,
        // so we need to first "download" it to the apps' cache folder.
        AndroidJavaClass unityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject> ("currentActivity");
        string gestureFilePath = activity.Call<AndroidJavaObject> ("getCacheDir").Call<string> ("getCanonicalPath");
        UnityWebRequest request = UnityWebRequest.Get (Application.streamingAssetsPath + "/" + gesturesFileName);
        request.SendWebRequest ();
        while (!request.isDone) {
            // wait for file extraction to finish
        }
        if (request.isNetworkError) {
            throw new Exception ("Failed to extract gestures.");
        }
        File.WriteAllBytes (_gesturesFilePath + "/" + gesturesFileName, request.downloadHandler.data);
#else
        // This will be the case when exporting a stand-alone PC app.
        // In this case, we can load the gesture database file from the streamingAssets folder.
        string _gesturesFilePath = Application.streamingAssetsPath;
#endif
        /*
        if (gr.loadFromFile(_gesturesFilePath + "/" + gesturesFileName) == false)
        {
            throw new Exception("Error loading gestures.");
        }

        */
    }

    // Update is called once per frame
    void Update()
    {
        NotifySensorialData();
        float triggerLeft = Input.GetAxis("LeftControllerTrigger");
        float triggerRight = Input.GetAxis("RightControllerTrigger");

        // If the user is not yet dragging (pressing the trigger) on either controller, he hasn't started a gesture yet.
        if (activeController == null)
        {
            // If the user presses either controller's trigger, we start a new gesture.
            if (triggerRight > 0.9)
            {
                // Right controller trigger pressed.
                activeController = rightHand;
            }
            else if (triggerLeft > 0.9)
            {
                // Left controller trigger pressed.
                activeController = leftHand;
            }
            else
            {
                // If we arrive here, the user is pressing neither controller's trigger:
                // nothing to do.
                return;
            }
            // If we arrive here: either trigger was pressed, so we start the gesture.
            Vector3 hmd_p = Camera.main.transform.position;
            Quaternion hmd_q = Camera.main.transform.rotation;
            //gr.startStroke(hmd_p, hmd_q);
        }

        // If we arrive here, the user is currently dragging with one of the controllers.
        Vector3 p = activeController.transform.position;
        Quaternion q = activeController.transform.rotation;
        //gr.contdStrokeQ(p, q);

        // Check if the user is still dragging or if he let go of the trigger button.
        if (triggerLeft < 0.85 && triggerRight < 0.85)
        {
            // the user let go of the trigger, ending a gesture.
            activeController = null;

            //DragStop(ref gr);
        }
    }

    private void DragStop(ref GestureRecognition gr)
    {
        var gesture = (InteractionMachine.Gesture)gr.endStroke(ref similarity, ref pos, ref scale, ref dir0, ref dir1, ref dir2);
        var cameraTransform = Camera.main.transform;
        if (gesture != InteractionMachine.Gesture.None)
        {
            subs.ForEach(i => i.Notify(gesture, cameraTransform.position, cameraTransform.forward));
        }
    }

    private void NotifySensorialData()
    {
        elapsed += Time.deltaTime;
        var period = 1 / frequency;
        if (elapsed >= period)
        {
            // Notify look direction
            var cameraTransform = Camera.main.transform;
            subs.ForEach(i => i.Notify(cameraTransform.position, cameraTransform.forward, period));

            // Reset elapsed time
            elapsed -= period;
        }
    }
}