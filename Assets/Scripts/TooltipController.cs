using UnityEngine;
using TMPro;

public class TooltipController : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI weightText;
    public TextMeshProUGUI valueText;
    public Vector3 tooltipOffset = new Vector3(0, 1.5f, 0);

    private Camera mainCamera;
    private Transform targetToFollow;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (targetToFollow != null)
        {
            Vector3 worldPos = targetToFollow.position + tooltipOffset;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            transform.position = screenPos;
        }
    }

    public void ShowTooltip(string itemName, int weight, int value, Transform target)
    {
        targetToFollow = target;
        nameText.text = itemName;
        weightText.text = "Peso: " + weight.ToString() + "kg";
        valueText.text = "Valor: $" + value.ToString();
        gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        targetToFollow = null;
        gameObject.SetActive(false);
    }
}