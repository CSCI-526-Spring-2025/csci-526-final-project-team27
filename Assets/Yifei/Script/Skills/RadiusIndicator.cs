using UnityEngine;

/// <summary>
/// 技能范围指示器脚本，用于显示技能范围的半透明圆形
/// </summary>
public class RadiusIndicator : MonoBehaviour
{
    [Header("外观设置")]
    [Tooltip("边框颜色")]
    public Color borderColor = new Color(1, 1, 1, 0.8f);
    
    [Tooltip("填充颜色")]
    public Color fillColor = new Color(0.5f, 0.5f, 1, 0.3f);
    
    [Tooltip("边框宽度")]
    public float borderWidth = 0.05f;

    private SpriteRenderer fillRenderer;
    private SpriteRenderer borderRenderer;

    private void Awake()
    {
        // 创建填充圆形
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(transform);
        fillObj.transform.localPosition = Vector3.zero;
        fillRenderer = fillObj.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = CreateCircleSprite();
        fillRenderer.color = fillColor;
        fillRenderer.sortingOrder = 97;

        // 创建边框圆形
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(transform);
        borderObj.transform.localPosition = Vector3.zero;
        borderRenderer = borderObj.AddComponent<SpriteRenderer>();
        borderRenderer.sprite = CreateRingSprite(borderWidth);
        borderRenderer.color = borderColor;
        borderRenderer.sortingOrder = 98;
    }

    /// <summary>
    /// 创建一个直径为2（半径为1）的圆形精灵
    /// 注意：我们希望单位圆的实际半径为1，这样当设置localScale为技能radius时，
    /// 可以得到正确大小的指示器圆形
    /// </summary>
    private Sprite CreateCircleSprite()
    {
        // 创建一个64x64的纹理
        int textureSize = 128;
        Texture2D texture = new Texture2D(textureSize, textureSize);

        
        // 设置透明度
        Color[] colors = new Color[textureSize * textureSize];
        
        int center = textureSize / 2;
        float radius = textureSize / 2f;
        
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                // 计算点到中心的距离
                float distance = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                
                // 如果在圆内，则设置为白色
                if (distance <= radius)
                {
                    colors[y * textureSize + x] = Color.white;
                }
                else
                {
                    colors[y * textureSize + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        // 创建精灵 - 重要：将pixelsPerUnit设置为textureSize/2，这样生成的Sprite在Unity世界中半径为1
        return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize);
    }

    /// <summary>
    /// 创建一个直径为2（半径为1）的环形精灵，与圆形精灵保持一致的尺寸
    /// </summary>
    private Sprite CreateRingSprite(float thickness)
    {
        // 创建一个64x64的纹理
        int textureSize = 128;
        Texture2D texture = new Texture2D(textureSize, textureSize);
        texture.filterMode = FilterMode.Bilinear;
        // 设置透明度
        Color[] colors = new Color[textureSize * textureSize];
        
        int center = textureSize / 2;
        float outerRadius = textureSize / 2f;
        float innerRadius = outerRadius - (thickness * textureSize);
        
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                // 计算点到中心的距离
                float distance = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                
                // 如果在环内，则设置为白色
                if (distance <= outerRadius && distance >= innerRadius)
                {
                    colors[y * textureSize + x] = Color.white;
                }
                else
                {
                    colors[y * textureSize + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        // 创建精灵 - 重要：将pixelsPerUnit设置为textureSize/2，这样生成的Sprite在Unity世界中半径为1
        return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize);
    }
} 