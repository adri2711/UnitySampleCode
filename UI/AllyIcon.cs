using UnityEngine;
using UnityEngine.UI;

public class AllyIcon : Billboard
{
    private Image mainIcon;
    private Image highlightIcon;
    [SerializeField] private float highlightIntensity = 2f;
    [SerializeField] private float alpha = .3f;

    private void OnEnable()
    {
        mainIcon = GetComponent<Image>();
        highlightIcon = transform.GetChild(0).GetChild(0).GetComponent<Image>();
    }

    public void SetColor(Vector4 color)
    {
        mainIcon.material = new Material(mainIcon.material.shader);
        mainIcon.material.color = color;
        
        float iMult = Mathf.Pow(2f,  highlightIntensity);
        Vector4 highlightColor = new Vector4(color.x * iMult, color.y * iMult, color.z * iMult, color.w * alpha);
        highlightIcon.material = new Material(highlightIcon.material.shader);
        highlightIcon.material.color = highlightColor;
    }
}
