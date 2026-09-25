using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SplashLogoController : MonoBehaviour
{
    [Header("UI Elements Transformers")]
    [SerializeField] private RectTransform baseLogoText;
    [SerializeField] private RectTransform snotDotElement;
    [SerializeField] private CanvasGroup globalSplashGroup;

    [Header("Motion Animation Timings")]
    [SerializeField] private Vector2 snotStartAnchorPos = new Vector2(0f,600f);
    [SerializeField] private Vector2 snotTargetAnchorPos = new Vector2(32f, 85f);
    [SerializeField] private float dropDuration = 0.8f;
    [SerializeField] private float splatBounceDuration = 0.4f;

    [Header("Juice & Audio Assets")]
    [SerializeField] private AudioClip snotImpactSplatSound;
    [SerializeField] private AnimationCurve dropCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private MenuController menuControllerUI;
    private void Start()
    {
        globalSplashGroup.alpha = 1f;
        snotDotElement.anchoredPosition = snotStartAnchorPos;
        snotDotElement.localScale = new Vector3(0.3f, 1.8f, 1.0f);

        StartCoroutine(ExecuteSpittyLogoSequence());
    }

    private IEnumerator ExecuteSpittyLogoSequence()
    {
        // Fade in the logo
        yield return new WaitForSeconds(0.3f); // inital delay Dark screen
        float elapsed = 0f;
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedProgress = elapsed / dropDuration;
            float curvedProgress = dropCurve.Evaluate(normalizedProgress);

            // interpolate vector pos smoothly down the ui frame canvas
            snotDotElement.anchoredPosition = Vector2.Lerp(snotStartAnchorPos, snotTargetAnchorPos, curvedProgress);
            // normalize scale length as it nears structural terminal Veloc traget
            snotDotElement.localScale = Vector3.Lerp(new Vector3(0.3f, 1.8f, 1.0f), new Vector3(1.0f, 1.0f, 1.0f), curvedProgress);
            yield return null;
        }

        snotDotElement.anchoredPosition = snotTargetAnchorPos;
        if (AudioManagerHub.Instance != null && snotImpactSplatSound != null)
        {
            AudioManagerHub.Instance.PlaySpatialExplosiveSFX(snotImpactSplatSound, Vector3.zero, 0.8f);
        }

        elapsed = 0f;

        Vector3 squishedScale = new Vector3(1.6f, 0.5f, 1.0f);
        Vector3 normalScale = new Vector3(1.0f, 1.0f, 1.0f);

        while (elapsed  < splatBounceDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedBounceProgress = elapsed / splatBounceDuration;
            //elastive sine wave
            float bounceScaleEvaluation = Mathf.Sin(normalizedBounceProgress * Mathf.PI * 2.5f) * (1f - normalizedBounceProgress);
            snotDotElement.localScale = normalScale + (squishedScale - normalScale) * bounceScaleEvaluation;
            baseLogoText.anchoredPosition = new Vector2(Random.Range(-3f, 3f), Random.Range(-3f, 3f));
           
            yield return null;
        }
        snotDotElement.localScale = normalScale;
        baseLogoText.anchoredPosition = Vector2.zero;
        yield return new WaitForSeconds(1.2f);

        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            globalSplashGroup.alpha = 1.0f - (elapsed / 0.5f);
            yield return null;
        }

        globalSplashGroup.alpha = 0f;
        gameObject.SetActive(false);

        if ( menuControllerUI != null)
        {
            menuControllerUI.ShowHomeScreen();
        }

    }
}