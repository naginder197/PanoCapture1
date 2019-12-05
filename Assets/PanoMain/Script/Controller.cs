using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using AOT;

public class Controller : MonoBehaviour
{

    static public Controller Instance;
    private const int imageLoadtime = 5; //secs

    public void Awake()
    {
        Instance = this;
    }
   
    //iOS Input call events
    enum PanoStatus_iOS { iOSIMAGE = 0, iOSCLOSE = 2, iOSROLL360 = 3 }

#if UNITY_IOS

    // iOS plugin inputs
    [DllImport("__Internal")]
    private static extern void present_custom_view();
    private delegate void DelegateMessage(int type, string path);

    [DllImport("__Internal")]
    private static extern void framework_setDelegate(DelegateMessage callback);

    [MonoPInvokeCallback(typeof(DelegateMessage))]
    private static void delegateMessageReceived(int type_event, string path)
    {
    
        if (type_event.Equals((int)PanoStatus_iOS.iOSROLL360)) //type.Equals((int)PanoStatus_android.PANORAMA_IMAGE)
        {
            Instance.LoadbarScreen.SetActive(false);
            Instance.GalleryScreen.SetActive(true);

        }
        if (type_event.Equals((int)PanoStatus_iOS.iOSCLOSE))
        {
            Instance.LoadbarScreen.SetActive(false);
            Instance.MainMenuScreen.SetActive(true);
        }
        if(type_event.Equals((int)PanoStatus_iOS.iOSIMAGE))
        {
            Instance.Imgagepath = path;
            Instance.StartCoroutine(Instance.GetTextureImage(path));
        }
    }

#endif

    //Can be used later for gallery view
    /*
     * public RawImage thumbnail_Template;
      public GameObject[] gameObj;
      */

    // android plugin input events
    enum PanoStatus_android { NO_ACTION = -1, PANORAMA_IMAGE = 0, BACK_PRESSED = 1, CLOSE = 2, ROLL_360 = 3 }


    public Material PreviewMaterial;
    public bool IsPluginCalled = false;
    public GameObject MainMenuScreen;
    public GameObject PreviewScreen;
    public GameObject GalleryScreen;
    public GameObject PopUpScreen;
    public GameObject PreviewObject;
    public GameObject LoadbarScreen;
    public string Imgagepath = null;
    const string urlPrefix = "file://";


    //iOS call back events
    public static void InitializeDelegate()
    {
#if UNITY_IOS
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
           framework_setDelegate(delegateMessageReceived);
        }
#endif
    }

    public static void PresentController()
    {
#if UNITY_IOS
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
          present_custom_view();
        }
#endif
    }

// plugin click events
    public void OnclickButton()
    {
#if UNITY_ANDROID

        IsPluginCalled = true;
        var androidJavaclass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var javaobject = androidJavaclass.GetStatic<AndroidJavaObject>("currentActivity");

        // Accessing the class to call a static method on it
        var pluginClass = new AndroidJavaClass("qt.wormhole.android.PluginClass");

        // Calling a Call method to which the current activity is passed
        pluginClass.CallStatic("startCaptureActivity", javaobject);
       

#else
    // iOS  plugin Input call event  
        PresentController();
        InitializeDelegate();

#endif
    }

    // android Plugin Input call events
    private void OnApplicationFocus(bool focus)
    {
#if UNITY_ANDROID
       
        if (focus && IsPluginCalled)
        {
            IsPluginCalled = false;
            var androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var javaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");

            // Accessing the class to call a static method on it
            var pluginClass = new AndroidJavaClass("qt.wormhole.android.PluginClass");

            // Calling a Call method to which the current activity is passed
            int type = pluginClass.CallStatic<int>("getActionType", javaObject);

            if (type.Equals((int)PanoStatus_android.PANORAMA_IMAGE))
            {
               
                Imgagepath = pluginClass.CallStatic<string>("getPanoramaImage", javaObject);
               
                StartCoroutine(GetTextureImage(Imgagepath));
            }
            if (type.Equals((int)PanoStatus_android.ROLL_360))
            {
              
               Instance.MainMenuScreen.SetActive(true);
            }
            if (type.Equals((int)PanoStatus_android.CLOSE))
            {
               Instance.GalleryScreen.SetActive(true);
            }
        }
#endif
    }

    // Image load from path
    public IEnumerator GetTextureImage(string path)
    {
    
        LoadbarScreen.SetActive(true);
        yield return new WaitForSeconds(imageLoadtime);
        var www = UnityWebRequestTexture.GetTexture(urlPrefix + path);
        yield return www.SendWebRequest();

        if (www.isDone)
        {
            if (File.Exists(path))
            {
                Texture2D imageTexture = DownloadHandlerTexture.GetContent(www);
                PreviewMaterial.SetTexture("_MainTex", imageTexture); /* _MainTex unity shader variable name*/
                LoadbarScreen.SetActive(false);
                MainMenuScreen.SetActive(false);
                PreviewScreen.SetActive(true);
                PreviewObject.SetActive(true);
            }
        }
    }

    public void OnclickKeepButton()
    {

        PreviewScreen.SetActive(false);
        PreviewObject.SetActive(false);
        GalleryScreen.SetActive(true);
    }

    public void OnclickRetakeButton()
    {
        PreviewScreen.SetActive(false);
        if (File.Exists(Imgagepath))
        {
            File.Delete(Imgagepath);
        }
        OnclickButton();
    }

    public void OnclickClosePreviewButton()
    {

        PreviewScreen.SetActive(false);
        MainMenuScreen.SetActive(true);
    }

    public void OnclickCloseGalleryButon()
    {
        GalleryScreen.SetActive(false);
        MainMenuScreen.SetActive(true);
    }

 }

