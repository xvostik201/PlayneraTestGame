using Coffee.UIExtensions;
using UnityEngine;

public class MakeupParticleController : MonoBehaviour
{
    public static MakeupParticleController Instance;

    [Header("Apply Particles")]
    [SerializeField] private UIParticle _applyParticle;           

    [Header("Loofan Particles")]
    [SerializeField] private UIParticle _loofanParticle;         

    private void Awake()
    {
        Instance = this;
    }

    public void PlayApplyParticles()
    {
        if (_applyParticle != null)
            _applyParticle.Play();
    }

    public void StopApplyParticles()
    {
        _applyParticle?.Stop();
    }

    public void PlayLoofanParticles()
    {
        if (_loofanParticle != null)
            _loofanParticle.Play();
    }

    public void PlayMakeupCompleteParticles()
    {
        _applyParticle?.Play();
    }
}