using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using Unity.VisualScripting;

[System.Serializable]
public class DataEntry
{
    public string key;
    public float value;
}

public class FirebaseDataUploader : MonoBehaviour
{
    [Header("Firebase 配置")]
    [Tooltip("Realtime Database URL（以 / 结尾）")]
    [SerializeField] private string firebaseURL = "https://cureallcrew-default-rtdb.firebaseio.com/CureAllCrew/";

    [Header("上传设置")]
    [Tooltip("上传间隔（秒）")]
    [SerializeField] private float uploadInterval = 5f;

    // 游戏启动时的时间戳（作为文档名称）
    private string sessionTimeStamp;
    // 浏览器信息
    private string browserInfo = "Unknown";
    // 记录游戏开始时的时间（用于计算游玩时长）
    private float startTime;
    // 设备唯一 ID
    private string deviceID;
    // OS 信息
    private string osInfo = "Unknown";
    // 分辨率
    private string resolution = "Unknown";

    //[Header("预设追踪的数据（键值对）")]
    //[SerializeField]
    private List<DataEntry> presetData = new List<DataEntry>()
    {
         new DataEntry() { key = "PlayTime", value = 0 },
         new DataEntry() { key = "EnemyKilled", value = 0 },
         new DataEntry() { key = "CoinCollected", value = 0 },
         new DataEntry() { key = "DifficultyLevelReached", value = 0 },
         new DataEntry() { key = "ExplorationRate", value = 0 },
    };

#if UNITY_WEBGL 
    [DllImport("__Internal")]
    private static extern void GetBrowserInfo(string gameObjectName);
#endif

    private void Start()
    {
        // 游戏开始时间及文档名称初始化
        startTime = Time.time;
        sessionTimeStamp = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        deviceID = SystemInfo.deviceUniqueIdentifier;
        osInfo = SystemInfo.operatingSystem;
        resolution = Screen.width + "x" + Screen.height;

#if UNITY_WEBGL 
        // 尝试获取浏览器信息
        try
        {
            GetBrowserInfo(gameObject.name);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("获取浏览器信息时出错: " + ex.Message);
            browserInfo = "Unavailable";
        }
#endif
        StartCoroutine(AutoUploadData());
    }

    /// <summary>
    /// 允许其他脚本调用此方法来更新预设数据中的某个字段
    /// </summary>
    public void UpdateData(string key, float value)
    {
        bool found = false;
        foreach (var entry in presetData)
        {
            if (entry.key.Equals(key))
            {
                entry.value = value;
                found = true;
                break;
            }
        }
        if (!found)
        {
            Debug.LogWarning("没有此数据: " + key);
        }
    }

    /// <summary>
    /// 允许其他脚本调用此方法来获取预设数据中的某个字段
    /// </summary>
    public float GetData(string key)
    {
        foreach (var entry in presetData)
        {
            if (entry.key.Equals(key))
            {
                return entry.value;
            }
        }
        Debug.LogWarning("没有此数据: " + key);
        return 0;
    }

    /// <summary>
    /// 允许其他脚本在需要时进行一次强制上传
    /// </summary>
    public void ForceUploadData()
    {
        UpdateData("PlayTime", Time.time - startTime);
        StartCoroutine(SendDataToFirebase());
    }

    /// <summary>
    /// 定期自动上传数据的协程
    /// </summary>
    private IEnumerator AutoUploadData()
    {
        while (true)
        {
            UpdateData("PlayTime", Time.time - startTime);
            yield return StartCoroutine(SendDataToFirebase());
            yield return new WaitForSeconds(uploadInterval);
        }
    }

    /// <summary>
    /// 构建最终上传的 JSON 字符串，包含预设数据和 GPU 信息
    /// </summary>
    private string BuildJson()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{");

        bool first = true;
        foreach (var entry in presetData)
        {
            if (!first)
                sb.Append(",");
            sb.Append("\"").Append(entry.key).Append("\":");
            sb.Append("\"").Append(entry.value).Append("\"");
            first = false;
        }

        # region 系统信息部分, 请勿修改
        // 添加系统信息部分
        sb.Append(",\"SystemInfo\":").Append("{");

        // 重置内部的 first 标志，避免前面 presetData 的影响
        bool innerFirst = true;

        // 浏览器信息
        if (!innerFirst)
            sb.Append(",");
        sb.Append("\"BrowserInfo\":").Append("\"").Append(browserInfo).Append("\"");
        innerFirst = false;

        // OS 信息
        if (!innerFirst)
            sb.Append(",");
        sb.Append("\"OSInfo\":").Append("\"").Append(osInfo).Append("\"");
        innerFirst = false;

        // 分辨率
        if (!innerFirst)
            sb.Append(",");
        sb.Append("\"Resolution\":").Append("\"").Append(resolution).Append("\"");

        sb.Append("}"); // 结束 SystemInfo 对象
        #endregion

        sb.Append("}"); // 结束整个 JSON 对象

        return sb.ToString();
    }


    /// <summary>
    /// 将当前 JSON 数据通过 PUT 请求上传到 Firebase
    /// </summary>
    private IEnumerator SendDataToFirebase()
    {
        string json = BuildJson();
        // 使用时间戳和 deviceID 作为节点名，每次 PUT 会覆盖该节点下的所有数据
        string requestURL = firebaseURL + sessionTimeStamp + "-" + deviceID + ".json";

        UnityWebRequest uwr = new UnityWebRequest(requestURL, "PUT");
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        uwr.uploadHandler = new UploadHandlerRaw(jsonBytes);
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("上传数据时出错: " + uwr.error);
        }
        else
        {
            Debug.Log("数据上传成功: " + uwr.downloadHandler.text);
        }
    }

    /// <summary>
    /// 在游戏退出时进行一次最终上传，不保证上传成功
    /// </summary>
    private void OnApplicationQuit()
    {
        ForceUploadData();
    }

    /// <summary>
    /// JS 回调此方法传回 GPU 信息
    /// </summary>
    public void OnBrowserInfoReceived(string info)
    {
        browserInfo = info;
    }
}
