using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    public Transform follow; // 需要跟随的对象
    public Camera followCamera; // 手动指定的相机
    public Vector2 roomPosition; // 当前房间的世界坐标
    public Vector2 roomSize; // 当前房间的大小
    public bool staticCamera = false; // 是否固定相机位置

    public enum FollowMode { Instant, Smooth }
    public FollowMode followMode = FollowMode.Smooth;

    public enum EaseType { Linear, EaseIn, EaseOut, EaseInOut }
    public EaseType easeType = EaseType.Linear;

    public float smoothSpeed = 5f; // 平滑移动速度

    private float camHalfWidth;
    private float camHalfHeight;

    float requiredOrthographicSize;

    void Start()
    {

        if (followCamera == null)
        {
            Debug.LogError("CameraFollow: followCamera is not assigned!");
            return;
        }
        // CalculateCameraBounds();
        requiredOrthographicSize = Mathf.Max(roomSize.y / 2, roomSize.x / (2 * followCamera.aspect));
        followCamera.orthographicSize = requiredOrthographicSize;
    }

    void LateUpdate()
    {
        if (follow == null || followCamera == null || staticCamera) return;

        Vector3 targetPos = follow.position;
        targetPos.z = transform.position.z; // 保持 Z 轴不变
        
         
        // 计算相机位置，使其不超出房间边界
        Vector3 clampedPosition = ClampCameraPosition(targetPos);

        // 根据不同模式应用相机移动
        if (followMode == FollowMode.Instant)
        {
            transform.position = clampedPosition;
        }
        else
        {
            transform.position = SmoothMove(transform.position, clampedPosition);
        }
    }

    // 限制相机视野在房间范围内
    private Vector3 ClampCameraPosition(Vector3 targetPos)
    {
        float roomLeft = roomPosition.x - roomSize.x / 2;
        float roomRight = roomPosition.x + roomSize.x / 2;
        float roomBottom = roomPosition.y - roomSize.y / 2;
        float roomTop = roomPosition.y + roomSize.y / 2;

        float clampedX = Mathf.Clamp(targetPos.x, roomLeft + camHalfWidth, roomRight - camHalfWidth);
        float clampedY = Mathf.Clamp(targetPos.y, roomBottom + camHalfHeight, roomTop - camHalfHeight);

        return new Vector3(clampedX, clampedY, targetPos.z);
    }


    // 计算相机边界
    private void CalculateCameraBounds()
    {
        if (followCamera == null) return;

        camHalfHeight = followCamera.orthographicSize;
        camHalfWidth = camHalfHeight * followCamera.aspect;

        //log half width and half height
        Debug.Log("camHalfWidth: " + camHalfWidth + " camHalfHeight: " + camHalfHeight);
    }

    // 平滑移动相机
    private Vector3 SmoothMove(Vector3 current, Vector3 target)
    {
        float t = smoothSpeed * Time.deltaTime;
        switch (easeType)
        {
            case EaseType.Linear:
                return Vector3.Lerp(current, target, t);
            case EaseType.EaseIn:
                return Vector3.Lerp(current, target, t * t);
            case EaseType.EaseOut:
                return Vector3.Lerp(current, target, 1 - Mathf.Pow(1 - t, 2));
            case EaseType.EaseInOut:
                return Vector3.Lerp(current, target, t * t * (3 - 2 * t));
            default:
                return target;
        }
    }

    // 更新当前房间信息
    public void UpdateRoomBounds(Vector2 newRoomPosition, Vector2 newRoomSize)
    {
        requiredOrthographicSize = Mathf.Max(roomSize.y / 2, roomSize.x / (2 * followCamera.aspect));
        followCamera.orthographicSize = requiredOrthographicSize;

        // roomPosition = newRoomPosition;
        // roomSize = newRoomSize;

        CalculateCameraBounds();

        if (staticCamera)
        {
            transform.position = new Vector3(newRoomPosition.x, newRoomPosition.y, transform.position.z);
            //MoveToPosition(newRoomPosition, 0.1f);
        }
    }

    // 一个协程，平滑移动相机到指定位置
    public void MoveToPosition(Vector3 targetPos, float duration)
    {
        StartCoroutine(MoveToPositionCoroutine(targetPos, duration));
    }

    private IEnumerator MoveToPositionCoroutine(Vector3 targetPos, float duration)
    {
        float t = 0;
        Vector3 startPos = transform.position;

        while (t < 1)
        {
            t += Time.deltaTime / duration;
            transform.position = SmoothMove(startPos, targetPos);
            yield return null;
        }
    }
}
