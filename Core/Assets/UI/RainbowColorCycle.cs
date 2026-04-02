using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class RainbowColorCycle : MonoBehaviour
{
    [SerializeField] Graphic targetGraphic;
    [SerializeField, Min(0f)] float cycleSpeed = 0.35f;
    [SerializeField, Range(0f, 1f)] float saturation = 1f;
    [SerializeField, Range(0f, 1f)] float value = 1f;
    [SerializeField] float hueOffset;
    [SerializeField] bool useUnscaledTime = true;

    void Reset()
    {
        targetGraphic = GetComponent<Graphic>();
    }

    void Awake()
    {
        if (targetGraphic == null)
        {
            targetGraphic = GetComponent<Graphic>();
        }
    }

    void Update()
    {
        if (targetGraphic == null) return;

        float time = useUnscaledTime ? Time.unscaledTime : Time.time;
        float hue = (time * cycleSpeed + hueOffset) % 1f;
        Color color = Color.HSVToRGB(hue, saturation, value);
        color.a = targetGraphic.color.a;
        targetGraphic.color = color;
    }
}
