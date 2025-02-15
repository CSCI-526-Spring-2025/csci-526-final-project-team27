using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerController_yf : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;                // ��ɫ�ƶ��ٶ�
    public Rigidbody2D rb;                      // ��ɫ�� Rigidbody2D
    public Camera mainCamera;                   // �������������ת�����λ��

    [Header("Attack Settings")]
    public Transform firePoint;                 // ����㣨�ӵ��ͼ���ͨ�������﷢�䣩
    public GameObject bulletPrefab;             // Զ���ӵ�Ԥ���壨����� Bullet �ű���
    public float bulletSpeed = 10f;
    public int bulletDamage = 10;

    [Header("Melee (Knockback) Attack Settings")]
    public GameObject meleeAttackPrefab;        // ��ս���˹���Ԥ���壨Ԥ������Ӧ�д����˺������˵��߼���
    public float meleeRange = 2f;               // ��ս��ⷶΧ
    public int meleeDamage = 15;                // ��ս�˺�
    public float meleeAngleThreshold = 30f;     // ���������ݲ�Ƕȣ�С�ڴ˽Ƕ���Ϊ�ڹ��������ڣ�

    [Header("Skill Settings")]
    [Tooltip("����Ԥ�����飨0��ֱ���ͷţ�1��2��Ŀ��/����ѡ��ģʽ��")]
    public GameObject[] skillPrefabs;           // ��������Ԥ�裨���滻��

    // ���ڼ����ͷŵ��ڲ�����
    private int selectedSkillIndex = -1;
    private bool isSkillTargetSelectionMode = false;

    void Update()
    {
        HandleMovement();
        //HandleRotation();

        // Ĭ�Ϲ�����������������δ���ڼ���ѡ��ģʽ
        if (!isSkillTargetSelectionMode && Input.GetMouseButtonDown(0))
        {
            Vector2 aimDir = GetAimDirection();
            // �����⵽���˴��ڽ�ս��Χ���ڹ��������ϣ���ִ�н�ս���˹�����������Զ���ӵ�
            if (IsEnemyInMeleeRange(aimDir))
                PerformMeleeAttack(aimDir);
            else
                ShootBullet(aimDir);
        }

        // �����ͷŴ��������ּ� 1~3 ���ƣ�
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // ���輼��1Ϊֱ���ͷ��ͼ���
            selectedSkillIndex = 0;
            UseSkill(selectedSkillIndex, GetAimDirection());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // ����2��Ҫ�Ȱ�������ȷ���ͷŷ���
            selectedSkillIndex = 1;
            isSkillTargetSelectionMode = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // ����3Ҳ����ѡ��ģʽ
            selectedSkillIndex = 2;
            isSkillTargetSelectionMode = true;
        }

        // ������ڼ���ѡ��ģʽ����ȴ���ҵ�����ȷ���ͷŷ���
        if (isSkillTargetSelectionMode && Input.GetMouseButtonDown(0))
        {
            Vector2 targetDir = GetAimDirection();
            UseSkill(selectedSkillIndex, targetDir);
            isSkillTargetSelectionMode = false;
        }
    }

    /// <summary>
    /// ��������ƶ�
    /// </summary>
    private void HandleMovement()
    {
        // ��ȡˮƽ�봹ֱ���루ע�⣺Input.GetAxisRaw ʹ���뼴ʱ��Ӧ��
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        rb.linearVelocity = moveInput * moveSpeed;
    }

    /// <summary>
    /// ��ɫ����������ڷ�����ת
    /// </summary>
    private void HandleRotation()
    {
        Vector2 aimDirection = GetAimDirection();
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        // ���ݽ�ɫ��ʼ���������ת�Ƕȣ��˴������ɫĬ�ϳ��ϣ��������90�㣩
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    /// <summary>
    /// ��ȡ��ɫ�������׼���������λ�þ�����
    /// </summary>
    /// <returns>��һ����2D����</returns>
    private Vector2 GetAimDirection()
    {
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;
        return direction;
    }

    /// <summary>
    /// ����Ƿ��е����ڽ�ս��Χ����λ�ڹ������������������
    /// </summary>
    /// <param name="attackDir">��������</param>
    /// <returns>�������򷵻� true������ false</returns>
    private bool IsEnemyInMeleeRange(Vector2 attackDir)
    {
        // �ڽ�ɫ��ǰλ���� meleeRange Ϊ�뾶�����ˣ�������˱��Ϊ "Enemy"��
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, meleeRange);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Vector2 enemyDir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                float angleDiff = Vector2.Angle(attackDir, enemyDir);
                if (angleDiff <= meleeAngleThreshold)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// ִ��Զ�̹����������ӵ�
    /// </summary>
    /// <param name="direction">��������</param>
    private void ShootBullet(Vector2 direction)
    {
        // ʵ�����ӵ�����ͨ�� Bullet �ű���ʼ�����ӵ� prefab Ӧ���� Bullet �ű���
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            //bulletScript.Initialize(direction, bulletSpeed, bulletDamage);
        }
    }

    /// <summary>
    /// ִ�н�ս���˹���
    /// </summary>
    /// <param name="direction">��������</param>
    private void PerformMeleeAttack(Vector2 direction)
    {
        // ʵ������ս����Ԥ���壨Ԥ���������д����˺������˵��߼���
        GameObject meleeAttack = Instantiate(meleeAttackPrefab, transform.position, Quaternion.identity);
        // ʹԤ���峯�򹥻�����
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        meleeAttack.transform.rotation = Quaternion.Euler(0, 0, angle);
        // Ԥ�����еĽű����� OnTriggerEnter2D �м����ˣ��������� Health.TakeDamage() ͬʱʩ�ӻ���Ч��
    }

    /// <summary>
    /// ʹ�ü��ܡ�����Ԥ�������һ����Ϊ Skill �Ľű������а��� Initialize(Vector2 direction) ������
    /// </summary>
    /// <param name="skillIndex">����������0��2��</param>
    /// <param name="direction">�ͷŷ���</param>
    private void UseSkill(int skillIndex, Vector2 direction)
    {
        if (skillIndex < 0 || skillIndex >= skillPrefabs.Length)
            return;

        GameObject skillInstance = Instantiate(skillPrefabs[skillIndex], firePoint.position, Quaternion.identity);
        // ʹ����Ч�������ͷŷ���
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        skillInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
        // �������Ԥ������ Skill �ű���������� Initialize ����
        //Skill skillScript = skillInstance.GetComponent<Skill>();
        //if (skillScript != null)
        //{
        //    skillScript.Initialize(direction);
        //}
    }

    #region Gizmos ���Ƶ��ԣ���ѡ��
    private void OnDrawGizmosSelected()
    {
        // ���ƽ�ս��ⷶΧ
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
    }
    #endregion
}
