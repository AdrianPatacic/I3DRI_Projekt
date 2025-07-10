using System;
using UnityEngine;

public class HudController : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private RectTransform crosshairRect;

    [SerializeField] float minScale = 0.5f;
    [SerializeField] float maxScale = 1.5f;
    [SerializeField] float maxDistance = 10f;

    void Update()
    {
        if (player.lockedOnPublic && player.lockedOnGameObjectPublic != null)
        {
            if (!crosshairRect.gameObject.activeSelf)
                crosshairRect.gameObject.SetActive(true);

            Vector3 targetPos = player.lockedOnGameObjectPublic.transform.position;
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
    }
}
