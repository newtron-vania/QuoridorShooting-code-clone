using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class LogManager : Singleton<LogManager>
{
    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }
    // List<string> _logs = new List<string>();
    string _filePath;
    string _fileName;
    void SaveLogToFile(string log)
    {
        try
        {
            if (!System.IO.Directory.Exists(_filePath))
            {
                System.IO.Directory.CreateDirectory(_filePath);
            }
            System.IO.File.AppendAllText(Path.Combine(_filePath, _fileName), log);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ERROR] LogManager::SaveLogToFile - Error writing log: {e}");
        }
    }
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // _logs.Add($"[{type}]{logString}");
        string log = $"[{type}] {logString}\n";
        switch (type)
        {
            case LogType.Error:
            case LogType.Assert:
            case LogType.Exception:
                {
                    // Debug.Log($"<color=red>[LogManager] {type}</color>: {logString}");
                    // Debug.Log($"<color=red>[LogManager]</color> Stack Trace: {stackTrace}");
                    // _logs.Add($"Stack Trace: {stackTrace}");
                    log += $"Stack Trace: {stackTrace}\n";
                    break;
                }
            case LogType.Warning:
            case LogType.Log:
                break;
        }
        SaveLogToFile(log);
    }
    public void Init()
    {
        _filePath = Application.persistentDataPath + "/logs/";
        _fileName = $"log-{System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}[{GameManager.Instance.RandSeed}]" + ".log";
        CrashReport crashReport = transform.AddComponent<CrashReport>();
        Debug.Log($"[INFO] LogManager::Init() - filePath: {_filePath}");
        Debug.Log($"[INFO] LogManager::Init() - fileName: {_fileName}");
        DeleteOldLogFiles(7); // 7일 이상된 로그파일 삭제
#if UNITY_ANDROID
        // 기기 모델명 가져오기
        string deviceModel = crashReport.CallAndroidFunction<string>("GetDeviceModel");
        Debug.Log($"[INFO] LogManager::Init() - deviceModel: {deviceModel}");

        // 안드로이드 버전 가져오기
        string androidVersion = crashReport.CallAndroidFunction<string>("GetAndroidVersion");
        Debug.Log($"[INFO] LogManager::Init() - androidVersion: {androidVersion}");
#endif
    }
    void DeleteOldLogFiles(int daysToKeep = 7)
    {
        if (!System.IO.Directory.Exists(_filePath))
        {
            return;
        }

        try
        {
            string[] files = System.IO.Directory.GetFiles(_filePath, "*.log");
            foreach (string file in files)
            {
                System.DateTime creationTime = System.IO.File.GetCreationTime(file);
                if ((System.DateTime.Now - creationTime).TotalDays > daysToKeep)
                {
                    System.IO.File.Delete(file);
                    Debug.Log($"Deleted old log file: {file}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }
    public string GetLatestLog()
    {
        string filePath = Path.Combine(_filePath, _fileName);
        if (File.Exists(filePath))
        {
            return File.ReadAllText(filePath);
        }
        else
        {
            Debug.LogError($"[ERROR] LogManager::GetLatestLog - Log file not found: {filePath}");
            return null;
        }
    }
}
