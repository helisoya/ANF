using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the Caribou Productions Splash Screen
/// </summary>
public class SplashScreen : MonoBehaviour
{
    [Header("Splash Screen")]
    [SerializeField] private Animator caribouAnimator;
    [SerializeField] private string nextScene;
    [SerializeField] private AudioSource source;

    public enum SoundType
    {
        CARIBOU,
        CAR,
        IMPACT
    }


    /// <summary>
	/// Ends the splash screen
	/// </summary>
    public void EndSplashScreen()
    {
        SceneManager.LoadScene(nextScene);
    }

    /// <summary>
	/// Plays a trigger in the Caribou Animator
	/// </summary>
	/// <param name="triggerName">The trigger's name</param>
    public void PlayCaribouTrigger(string triggerName)
    {
        caribouAnimator.SetTrigger(triggerName);
    }

    /// <summary>
	/// Play a 2D Sound
	/// </summary>
	/// <param name="clip">The clip</param>
    public void Play2DSound(AudioClip clip)
    {
        source.PlayOneShot(clip);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            EndSplashScreen();
    }
}
