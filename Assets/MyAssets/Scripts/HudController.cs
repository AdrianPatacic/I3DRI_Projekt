using System;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private RectTransform crosshairRect;
    [SerializeField] private Slider staminaBar;
    [SerializeField] private Slider healthBar;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject deathPanel;

    [SerializeField] float minScale = 0.2f;
    [SerializeField] float maxScale = 1f;
    [SerializeField] float maxDistance = 10f;

    private void Start()
    {
        if (staminaBar == null)
            staminaBar = transform.Find("StaminaBar").GetComponent<Slider>();

        if (healthBar == null)
            healthBar = transform.Find("HealthBar").GetComponent<Slider>();

        if (staminaBar != null)
            staminaBar.maxValue = player.GetMaxStamina();

        if (healthBar != null)
            healthBar.maxValue = player.GetMaxHealth();
    }

    void Update()
    {
        if (player.lockedOnPublic && player.lockedOnGameObjectPublic != null)
        {
            if (!crosshairRect.gameObject.activeSelf)
                crosshairRect.gameObject.SetActive(true);

            Vector3 targetPos = player.lockedOnGameObjectPublic.transform.position;
            targetPos.y += 1f;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(targetPos);
            crosshairRect.position = screenPos;

            float distance = Vector3.Distance(mainCamera.transform.position, targetPos);

            float t = Mathf.Clamp01(1f - (distance / maxDistance));
            float scale = Mathf.Lerp(minScale, maxScale, t);

            crosshairRect.localScale = Vector3.one * scale;
        }
        else
        {
            if (crosshairRect.gameObject.activeSelf)
                crosshairRect.gameObject.SetActive(false);
        }

        staminaBar.value = player.GetStamina();
        healthBar.value = player.GetHealth();

        if (Input.GetKeyDown(KeyCode.Escape) && pauseMenuPanel.activeSelf)
        {
            Resume();
        }else if(Input.GetKeyDown(KeyCode.Escape) && !pauseMenuPanel.activeSelf)
        {
            Pause();
        }

    }

    public void Resume()
    {
        
        if (!player.dead)
        {
            Time.timeScale = 1f;
        }
        pauseMenuPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        pauseMenuPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void EnableDeathScreen()
    {
        deathPanel.SetActive(true);
    }
}
