using Photon.Pun;
using UnityEngine;

public class PlayerHealth : MonoBehaviourPun
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Health Bars (for scaling)")]
    public Transform[] healthBars;

    [Header("Billboard Objects (for rotation)")]
    public Transform[] billboardObjects;

    [Header("Camera")]
    public Camera cam;

    private Vector3[] initialScales;

    private void Start()
    {
        currentHealth = maxHealth;

        if (cam == null)
            cam = Camera.main;

        // Сохраняем начальные масштабы только для тех, кто меняет размер
        if (healthBars != null)
        {
            initialScales = new Vector3[healthBars.Length];
            for (int i = 0; i < healthBars.Length; i++)
            {
                if (healthBars[i] != null)
                    initialScales[i] = healthBars[i].localScale;
            }
        }

        UpdateHealthBars();
    }

    private void LateUpdate()
    {
        // Поворачиваем только те объекты, которые указаны в billboardObjects
        if (billboardObjects == null || cam == null) return;

        for (int i = 0; i < billboardObjects.Length; i++)
        {
            if (billboardObjects[i] == null) continue;

            billboardObjects[i].rotation = Quaternion.LookRotation(cam.transform.forward);
        }
    }

    [PunRPC]
    public void TakeDamageRPC(int damage)
    {
        if (!photonView.IsMine)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        photonView.RPC(nameof(SyncHealthRPC), RpcTarget.Others, currentHealth);

        UpdateHealthBars();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    [PunRPC]
    private void SyncHealthRPC(int newHealth)
    {
        currentHealth = newHealth;
        UpdateHealthBars();
    }

    void UpdateHealthBars()
    {
        if (healthBars == null) return;

        float percent = (float)currentHealth / maxHealth;

        for (int i = 0; i < healthBars.Length; i++)
        {
            if (healthBars[i] == null) continue;

            Vector3 scale = initialScales[i];
            scale.x = initialScales[i].x * percent;

            healthBars[i].localScale = scale;
        }
    }

    void Die()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}