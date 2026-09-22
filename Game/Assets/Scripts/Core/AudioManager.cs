using System.Collections.Generic;
using UnityEngine;

public class AudioManagerHub : MonoBehaviour
{
    public static AudioManagerHub Instance;

    [Header("Audio Performance Configurations")]
    [SerializeField] private int audioPoolSize = 20; // Increased size to handle simultaneous weapon explosions + cat chaos
    [SerializeField] private GameObject audioSourcePrefab;

    private List<AudioSource> availableSourcesPool = new List<AudioSource>();

    private void Awake()
    {
        // Global singleton persistence configuration across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSystemPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Spawns the performance-optimized audio clip buffer grid layout safely on startup.
    /// </summary>
    private void InitializeAudioSystemPool()
    {
        for (int i = 0; i < audioPoolSize; i++)
        {
            GameObject soundObj = new GameObject($"PooledAudioSource_{i}");
            soundObj.transform.SetParent(transform);
            
            AudioSource source = soundObj.AddComponent<AudioSource>();
            
            // Set default high-performance universal mixing rules
            source.playOnAwake = false;
            source.spatialBlend = 1.0f; // Force 100% 3D spatial calculations by default
            
            soundObj.SetActive(false);
            availableSourcesPool.Add(source);
        }
    }

    /// <summary>
    /// Fetches an inactive audio channel frame from the cached array, places it dynamically in 3D coordinates, and fires.
    /// </summary>
    public void PlaySpatialExplosiveSFX(AudioClip clip, Vector3 worldPosition, float volume = 1f, float pitchRandomness = 0.12f)
    {
        if (clip == null) return;

        // Hunt down a free channel element that isn't currently outputting wave data lines
        AudioSource freeSource = availableSourcesPool.Find(src => !src.gameObject.activeInHierarchy);
        
        if (freeSource == null)
        {
            // Performance fallback safety: hijack the oldest active channel segment, cancel its playback path, and reuse it
            freeSource = availableSourcesPool[0];
            freeSource.Stop();
        }

        freeSource.gameObject.SetActive(true);
        freeSource.transform.position = worldPosition;
        
        freeSource.clip = clip;
        freeSource.volume = volume;
        
        // Minor dynamic pitch variance prevents duplicate sound overlapping audio fatigue
        freeSource.pitch = 1f + Random.Range(-pitchRandomness, pitchRandomness);

        freeSource.Play();
        StartCoroutine(DisableSourceAfterPlayback(freeSource, clip.length));
    }

    private System.Collections.IEnumerator DisableSourceAfterPlayback(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (source != null)
        {
            source.gameObject.SetActive(false);
        }
    }
}
