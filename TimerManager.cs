using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerManager : MonoBehaviour
{
    public Image targetImage;
    public float[] durationTimers;
    public TextMeshProUGUI timerText;
    private float elapsedTime = 0f;
    private float duration;
    private int index = 0;

    private void Start()
    {
        if (durationTimers.Length > 0)
        {
            duration = durationTimers[index];
        }
    }

    void FixedUpdate()
    {
        if (index < durationTimers.Length)
        {
            if (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float fillValue = Mathf.Clamp01(elapsedTime / duration);
                targetImage.fillAmount = fillValue;

                float remainingTime = Mathf.Max(0, duration - elapsedTime);
                int minutes = Mathf.FloorToInt(remainingTime / 60f);
                int seconds = Mathf.FloorToInt(remainingTime % 60f);

                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
            else
            {
                ChangeTimer();
            }
        }
    }

    void ChangeTimer()
    {
        index++;
        if (index < durationTimers.Length)
        {
            duration = durationTimers[index];
            elapsedTime = 0f;
        }
    }
}
