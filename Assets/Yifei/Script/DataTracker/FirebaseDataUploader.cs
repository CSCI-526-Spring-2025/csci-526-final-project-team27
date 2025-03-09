using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

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
    // 记录游戏开始时的时间（用于计算游玩时长）
    private float startTime;

    [Header("预设追踪的数据（键值对）")]
    [SerializeField]
    private List<DataEntry> presetData = new List<DataEntry>()
    {
         new DataEntry() { key = "PlayTime", value = 0 },
         new DataEntry() { key = "EnemyKilled", value = 0 },
    };

    private void Start()
    {
        // 游戏开始时间戳
        startTime = Time.time;
        // 用当前时间生成一个时间戳，作为文档名称
        sessionTimeStamp = System.DateTime.Now.ToString("yyyyMMddHHmmss");
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
        // 更新最新的游玩时长
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
            // 在每次上传前更新游玩时间
            UpdateData("PlayTime", Time.time - startTime);
            yield return StartCoroutine(SendDataToFirebase());
            yield return new WaitForSeconds(uploadInterval);
        }
    }

    /// <summary>
    /// 将当前 presetData 转换为 JSON 格式，并通过 PUT 请求上传到 Firebase
    /// </summary>
    private IEnumerator SendDataToFirebase()
    {
        string json = ListToJson(presetData);
        // 使用时间戳作为节点名，每次 PUT 会覆盖该节点下的所有数据
        string requestURL = firebaseURL + sessionTimeStamp + ".json";

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
    /// 将预设数据列表转换为 JSON 格式
    /// </summary>
    private string ListToJson(List<DataEntry> dataList)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        bool first = true;
        foreach (var entry in dataList)
        {
            if (!first)
                sb.Append(",");
            sb.Append("\"").Append(entry.key).Append("\":");
            sb.Append("\"").Append(entry.value).Append("\"");
            first = false;
        }
        sb.Append("}");
        return sb.ToString();
    }

    /// <summary>
    /// 在游戏退出时进行一次最终上传，不一定有用
    /// </summary>
    private void OnApplicationQuit()
    {
        ForceUploadData();
    }
}
