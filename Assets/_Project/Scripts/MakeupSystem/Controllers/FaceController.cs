using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class FaceController : MonoBehaviour
{
    public static FaceController Instance;

    [Header("Layers")]
    [SerializeField] private Image _lipstickLayer;
    [SerializeField] private Image _eyeshadowLayer;
    [SerializeField] private Image _blushLayer;

    [Header("Acne")]
    [SerializeField] private GameObject _acne;

    private Dictionary<MakeupTool, Image> _layers = new();
    private Dictionary<MakeupTool, float> _progresses = new();

    private void Awake()
    {
        Instance = this;

        _layers = new Dictionary<MakeupTool, Image>
        {
            { MakeupTool.Lipstick,   _lipstickLayer },
            { MakeupTool.Eyeshadow,  _eyeshadowLayer },
            { MakeupTool.Blush,      _blushLayer }
        };

        _progresses = new Dictionary<MakeupTool, float>
        {
            { MakeupTool.Lipstick,   0f },
            { MakeupTool.Eyeshadow,  0f },
            { MakeupTool.Blush,      0f }
        };
    }

    public void ApplyCream()
    {
        _acne.SetActive(false);
    }

    public void ApplyMakeup(MakeupData _data)
    {
        if (_layers.TryGetValue(_data.type, out Image _layer))
        {
            _layer.sprite = _data.applySprite;
            _layer.enabled = true;
        }
    }

    public void Clear()
    {
        Sequence fadeSequence = DOTween.Sequence();

        foreach (Image layer in _layers.Values)
        {
            if (layer != null && layer.enabled)
            {
                fadeSequence.Join(layer.DOFade(0f, 0.8f));
            }
        }

        fadeSequence.OnComplete(() =>
        {
            foreach (Image layer in _layers.Values)
            {
                if (layer != null)
                {
                    layer.enabled = false;
                    layer.color = Color.white;        
                    layer.fillAmount = 0f;      
                }
            }

            _acne.SetActive(true);

            if (_progresses.ContainsKey(MakeupTool.Lipstick))   _progresses[MakeupTool.Lipstick] = 0f;
            if (_progresses.ContainsKey(MakeupTool.Eyeshadow))  _progresses[MakeupTool.Eyeshadow] = 0f;
            if (_progresses.ContainsKey(MakeupTool.Blush))      _progresses[MakeupTool.Blush] = 0f;
        });
    }

    public void ApplyMakeupProgress(MakeupData _data, float _deltaTime, MakeupTool _type)
    {
        if (_layers.TryGetValue(_type, out Image _layer) && _progresses.ContainsKey(_type))
        {
            ApplyProgress(_data, _deltaTime, _layer, _type);
        }
    }

    private void ApplyProgress(MakeupData _data, float _deltaTime, Image _layer, MakeupTool _type)
    {
        _layer.enabled = true;
        _layer.sprite = _data.applySprite;

        float previousProgress = _progresses[_type];          
        float progress = _progresses[_type];
        progress += _deltaTime * 0.5f;
        progress = Mathf.Clamp01(progress);
        _progresses[_type] = progress;

        _layer.fillAmount = progress;

        if (previousProgress < 1f && progress >= 1f)
        {
            MakeupParticleController.Instance.PlayApplyParticles();

            if (_type == MakeupTool.Eyeshadow || _type == MakeupTool.Blush)
            {
                HandController.Instance.ReturnBrushAndToDefault();
            }
            else
            {
                HandController.Instance.FinishMakeupApplication();
            }
        }
    }

    public void ResetMakeup(MakeupData data)
    {
        if (_progresses.ContainsKey(data.type))
        {
            ResetMakeupProgress(data.type);
        }
    }

    private void ResetMakeupProgress(MakeupTool type)
    {
        _progresses[type] = 0f;

        if (_layers.TryGetValue(type, out Image layer) && layer != null)
        {
            layer.fillAmount = 0f;
        }
    }
}