using UnityEngine;
using TMPro;
using System.Collections;
using System.IO;
using System.Diagnostics; // for Process
using Debug = UnityEngine.Debug;
using System.Collections.Concurrent;

public class DataUploadSceneHandler : MonoBehaviour
{
    public TextMeshProUGUI dataStatus;
    public string status = null;
    private bool hasUploaded = false; // ensures script runs only 
    private ConcurrentQueue<System.Action> _actionQueue = new ConcurrentQueue<System.Action>();
    

    void Start()
    {
        Debug.Log($"status : {DataManager.status}");
            // PlutoComm.stopSensorStream();

            // ConnectToRobot.disconnect();
        // });
        // Start a coroutine that checks file every 60 seconds
            StartCoroutine(CheckUploadStatusRoutine());
    }

    void Update()
    {
        if (DataManager.status != "no_upload")
        {
            dataStatus.text = "Data is Uploading...";
            dataStatus.color = Color.green;
        }
            // dataStatus.text = $"{DataManager.status}";

    
    }
    IEnumerator CheckUploadStatusRoutine()
    {
        while (true)
        {
            AppData.Instance.userData.ReadFile();
            // dataStatus.text = $"{DataManager.status}";
            if (DataManager.status == "upload_needed" && !hasUploaded)
            {
                hasUploaded = true;
                Debug.Log("Upload started...");
                RunPythonUploader();
            }
            else if (DataManager.status == "no_upload" && hasUploaded)
            {
                Debug.Log("Upload completed. Shutting down...");
                ShutdownSystem();
                yield break;
            }

            yield return new WaitForSeconds(60f); // wait 1 minute
        }
    }

    

    void RunPythonUploader()
    {
        string pythonScriptPath = @"C:/pythonscripts/uploadToAWS.pyw";
        // string pythonExecutionPath = @"C:/Users/Homer 6/AppData/Local/Programs/Python/Python313/pythonw.exe";
        string pythonExecutionPath = @"C:/Program Files/Python312/pythonw.exe";

        if (!File.Exists(pythonScriptPath))
        {
            Debug.LogError("Python script not found: " + pythonScriptPath);
            return;
        }

        try
        {
            Process process = new Process();
            process.StartInfo.FileName = pythonExecutionPath;
            process.StartInfo.Arguments = $"\"{pythonScriptPath}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error running Python script: " + ex.Message);
        }
    }

    void ShutdownSystem()
    {
        try
        {
            Application.Quit();
            // Process.Start("shutdown", "/s /t 0");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false; 
            #endif
            // Process.Start("shutdown", "/s /t 0");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to shutdown: " + ex.Message);
        }
    }
}
