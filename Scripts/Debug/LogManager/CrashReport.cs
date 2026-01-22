using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


// 아직 작업중입니다.
// 나중에 메일 보내는 기능을 구현할 예정입니다. (동현)
public class CrashReport : MonoBehaviour
{
    public struct Report
    {
        public string Name;
        public string Email;
        public string Type;
        public string DeviceModel;
        public string AndroidVersion;
        public string Title;
        public string Message;
        public string Log;

        public Report(string name, string email, string type, string deviceModel, string androidVersion, string title, string message, string log)
        {
            this.Name = name;
            this.Email = email;
            this.Type = type;
            this.DeviceModel = deviceModel;
            this.AndroidVersion = androidVersion;
            this.Title = title;
            this.Message = message;
            this.Log = log;
        }
    }
    void Update()
    {
        // #if UNITY_EDITOR || DEVELOPMENT_BUILD
        //         if (Input.GetKeyDown(KeyCode.P))
        //         {
        //             Send("이름", "이메일",, "제목", "내용");
        //         }
        // #endif
    }
    public void Send(string name, string email, string type, string title, string message)
    {
#if UNITY_ANDROID

        string deviceModel = CallAndroidFunction<string>("GetDeviceModel");
        string androidVersion = CallAndroidFunction<string>("GetAndroidVersion");
#else
        string deviceModel = SystemInfo.deviceModel;
        string androidVersion = SystemInfo.operatingSystem;
#endif
        string log = LogManager.Instance.GetLatestLog();
        if (string.IsNullOrEmpty(log))
        {
            log = "No log available.";
        }
        Report report = new Report(name, email, type, deviceModel, androidVersion, title, message, log);
        StartCoroutine(SendReport(report));

    }
    IEnumerator SendReport(Report report)
    {
        string endpoint = "https://windmill.dony910.kro.kr/api/w/hm/jobs/run/f/u/lovegameoh/quoridor";
        string json = JsonUtility.ToJson(report);

        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(endpoint, "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer 5sdjbEs9jz0s1eTVVqLx2acKdKaSDWkS");

        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("[ERROR] CrashReport::SendReport - " + request.error);
        }
        else
        {
            Debug.Log("[INFO] CrashReport::SendReport - Response: " + request.downloadHandler.text);
        }
        request.Dispose();
    }
    // Android Java 클래스명
    private const string androidClassName = "com.HoegidongMakguli.QuoridorShooting.DeviceInfo";

    // Android Java 함수 호출하는 메서드
    public T CallAndroidFunction<T>(string functionName)
    {
#if UNITY_ANDROID
        AndroidJavaClass androidClass = new AndroidJavaClass(androidClassName);
        try
        {
            return androidClass.CallStatic<T>(functionName);
        }
        catch (AndroidJavaException e)
        {
            Debug.LogError("[ERROR] LogManager::CallAndroidFunction - Android Java Exception: " + e.Message);
            return default(T);
        }
        finally
        {
            androidClass.Dispose();
        }
#else
        Debug.LogError("[ERROR] LogManager::CallAndroidFunction - This function can only be called on Android.");
        return default(T);
#endif
    }
}
