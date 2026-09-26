using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SplashLogoController : MonoBehaviour
{
    [Header("UI Elements Transformers")]
    [SerializeField] private RectTransform baseLogoText;
    [SerializeField] private RectTransform snotDotElement;
    [SerializeField] private Image snotImageComponent;
    [SerializeField] private CanvasGroup globalSplashGroup;

    [Header("Motion Animation Timings")]
    private Vector2 snotStartAnchorPos;
    private Vector2 snotTargetAnchorPos;
    [SerializeField] private float dropDuration = 0.8f;
    [SerializeField] private float splatBounceDuration = 0.4f;

    [Header("Juice & Audio Assets")]
    [SerializeField] private AudioClip snotImpactSplatSound;
    [SerializeField] private AnimationCurve dropCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private MenuController menuControllerUI;

    [Header(" Slime Morphing")]
    [SerializeField] private Sprite snotSplatSprite; 

    private void Start()
    {
        if (globalSplashGroup != null) globalSplashGroup.alpha = 1f;

        // AUTOMATIC CATCH: If the inspector slot was left empty, find it automatically!
        if (snotImageComponent == null && snotDotElement != null)
        {
            snotImageComponent = snotDotElement.GetComponent<Image>();
        }

        // FORCE CORRECT VECTORS: Straight drop down the vertical center axis line
        snotStartAnchorPos = new Vector2(0f, 600f);
        snotTargetAnchorPos = new Vector2(-19.3f, 12.79f); // Centered straight over the "I" stem

        // Fix hex coloring safely using Unity's built-in converter
        if (snotImageComponent != null)
        {
            Color initialColor;
            if (ColorUtility.TryParseHtmlString("#1DFA35", out initialColor))
            {
                snotImageComponent.color = initialColor;
            }
            else
            {
                snotImageComponent.color = Color.white;
            }
        }

        // Align pivots to center so stretching down doesn't deflect coordinates sideways
        if (snotDotElement != null)
        {
            snotDotElement.pivot = new Vector2(0.5f, 0.5f);
            snotDotElement.anchoredPosition = snotStartAnchorPos;
            snotDotElement.localScale = new Vector3(0.6f, 1.6f, 1.0f); // Clean initial stretched drop shape
        }

        StartCoroutine(ExecuteSpittyLogoSequence());
    }

    private IEnumerator ExecuteSpittyLogoSequence()
    {
        yield return new WaitForSeconds(0.3f); // Initial delay dark screen
        
        float elapsed = 0f;
        Vector3 initialDropScale = new Vector3(0.6f, 1.6f, 1.0f);
        Vector3 terminalDropScale = new Vector3(0.8f, 1.2f, 1.0f);

        // 1. FALL PHASE
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedProgress = elapsed / dropDuration;
            float curvedProgress = dropCurve.Evaluate(normalizedProgress);

            if (snotDotElement != null)
            {
                snotDotElement.anchoredPosition = Vector2.Lerp(snotStartAnchorPos, snotTargetAnchorPos, curvedProgress);
                snotDotElement.localScale = Vector3.Lerp(initialDropScale, terminalDropScale, curvedProgress);
            }
            yield return null;
        }

        if (snotDotElement != null) snotDotElement.anchoredPosition = snotTargetAnchorPos;

        // 2. IMPACT FLASH: Swap sprite texture to the splat and inject the toxic neon green color values
        if (snotImageComponent != null && snotSplatSprite != null)
        {
            snotImageComponent.sprite = snotSplatSprite;
            
            Color impactColor;
            if (ColorUtility.TryParseHtmlString("#26E63B", out impactColor))
            {
                snotImageComponent.color = impactColor;
            }
            else
            {
                snotImageComponent.color = new Color(0.18f, 0.80f, 0.24f, 1.0f);
            }
        }

        if (AudioManagerHub.Instance != null && snotImpactSplatSound != null)
        {
            AudioManagerHub.Instance.PlaySpatialExplosiveSFX(snotImpactSplatSound, Vector3.zero, 0.8f);
        }

        // 3. JUICY SQUASH & STRETCH BOUNCE PHASE
        elapsed = 0f;
        Vector3 squishedScale = new Vector3(1.8f, 0.4f, 1.0f); 
        Vector3 normalScale = new Vector3(1.0f, 1.0f, 1.0f);
        Vector2 textOriginalAnchorPos = baseLogoText != null ? baseLogoText.anchoredPosition : Vector2.zero;

        while (elapsed < splatBounceDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedBounceProgress = elapsed / splatBounceDuration;
            
            float bounceScaleEvaluation = Mathf.Sin(normalizedBounceProgress * Mathf.PI * 2.5f) * (1f - normalizedBounceProgress);
            
            if (snotDotElement != null)
            {
                snotDotElement.localScale = normalScale + (squishedScale - normalScale) * bounceScaleEvaluation;
            }
            
            if (baseLogoText != null)
            {
                float shakeStrength = 6f * (1f - normalizedBounceProgress);
                baseLogoText.anchoredPosition = textOriginalAnchorPos + new Vector2(Random.Range(-shakeStrength, shakeStrength), Random.Range(-shakeStrength, shakeStrength));
            }
           
            yield return null;
        }

        if (snotDotElement != null) snotDotElement.localScale = normalScale;
        if (baseLogoText != null) baseLogoText.anchoredPosition = textOriginalAnchorPos;
        
        yield return new WaitForSeconds(1.2f); 

        // 4. FADE OUT EXIT PHASE
         elapsed = 0f;
        float textFadeDuration = 0.5f;

        // Step A: Disconnect the Text from the canvas group or fade it independently
        // If your text components are wrapped, we can manually fade its color channel transparency alpha
        Graphic textComponent = baseLogoText.GetComponent<Graphic>();
        Graphic productionsTextComponent = baseLogoText.transform.parent.Find("Productions")?.GetComponent<Graphic>(); 
        // Note: Replace "Productions" with your exact secondary text object name if split!

        while (elapsed < textFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / textFadeDuration;

            // Smoothly dissolve just the text labels away into the dark purple matrix background
            if (textComponent != null) textComponent.color = Color.Lerp(textComponent.color, new Color(1f, 1f, 1f, 0f), t);
            
            yield return null;
        }

        // Step B: Slime Surge! Rocket the snot dot directly forward along the Z/Scale plane
        elapsed = 0f;
        float surgeDuration = 0.4f;
        Vector3 initialSurgeScale = snotDotElement.localScale;
        
        // Massive destination profile matrix to swallow a standard 1080p canvas grid layout entirely
        Vector3 terminalSurgeScale = new Vector3(120f, 120f, 1.8f); 

        while (elapsed < surgeDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedSurgeProgress = elapsed / surgeDuration;
            
            // Fast exponential acceleration curve mimicking a rapid physics explosive force rush
            float curvedSurgeProgress = normalizedSurgeProgress * normalizedSurgeProgress * normalizedSurgeProgress;

            if (snotDotElement != null)
            {
                snotDotElement.localScale = Vector3.Lerp(initialSurgeScale, terminalSurgeScale, curvedSurgeProgress);
            }
            yield return null;
        }

        yield return new WaitForSeconds(0.1f); // Brief moment of total blindness while screen is covered in green slime

        // Step C: Hand execution control cleanly over to your physical Main Menu level scene zone
        Debug.Log("[SplashLogoController] Screen swallowed by snot. Swapping level layers now...");
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
