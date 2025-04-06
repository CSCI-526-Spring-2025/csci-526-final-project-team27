using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance;

    [Header("经验设置")]
    public int currentLevel = 1;
    public int currentExperience = 0;
    public int baseExperienceToLevel = 100;
    public float levelMultiplier = 1.5f;

    private int experienceToNextLevel;
    public LevelUpRewardSystem rewardSystem;

    [Header("UI组件")]
    public Image expFillImage;             // 绿色经验条（Image类型，需设置为 Filled）
    public TextMeshProUGUI levelText;                // 菱形上的等级文本

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        experienceToNextLevel = CalculateExpToNextLevel();
        UpdateUI();
    }

    public void AddExperience(int amount)
    {
        currentExperience += amount;
        while (currentExperience >= experienceToNextLevel)
        {
            currentExperience -= experienceToNextLevel;
            LevelUp();
            experienceToNextLevel = CalculateExpToNextLevel();
        }
        UpdateUI();
    }

    private void LevelUp()
    {
        currentLevel++;
        rewardSystem.ShowRewardSelection();
        Debug.Log("升级啦！当前等级：" + currentLevel);
    }

    private int CalculateExpToNextLevel()
    {
        return Mathf.RoundToInt(baseExperienceToLevel * Mathf.Pow(levelMultiplier, currentLevel - 1));
    }

    private void UpdateUI()
    {
        if (expFillImage != null)
        {
            float progress = (float)currentExperience / experienceToNextLevel;
            expFillImage.fillAmount = progress;
        }
        if (levelText != null)
        {
            levelText.text = currentLevel.ToString();
        }
    }
}
