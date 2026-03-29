using System;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;

public class HandController : MonoBehaviour
{
    public static HandController Instance;

    [Header("Hand")]
    [SerializeField] private RectTransform _handRect;
    [SerializeField] private Vector2 _dragOffset = new Vector2(-30, -40);
    [SerializeField] private float _dragSmooth = 15f;
    [SerializeField] private Vector2 _defaultPos = new Vector2(400, -1200);

    [Header("Intermediate positions")]
    [SerializeField] private RectTransform _afterTakePosition;

    [Header("Tool Pickup Points")]
    [SerializeField] private RectTransform _eyeBrushPickupPoint;
    [SerializeField] private RectTransform _blushBrushPickupPoint;
    [SerializeField] private RectTransform _chestPosition;

    [Header("Palette Brushes to Hide")]
    [SerializeField] private GameObject _eyeBrushPaletteObject;   
    [SerializeField] private GameObject _blushBrushPaletteObject; 

    [Header("Dipping Settings")]
    [SerializeField] private float _dippingDepth = 40f;
    [SerializeField] private float _dippingPause = 0.45f;

    [Header("Hand Visuals")]
    [SerializeField] private Image[] _lipsticks;
    [SerializeField] private Image _cream;
    [SerializeField] private Image _loofan;
    [SerializeField] private Image _blushBrush;
    [SerializeField] private Image _eyeBrush;

    [Header("Loofan anim")]
    [SerializeField] private RectTransform[] _loofanAnimPosition;
    [SerializeField] private float _stepTime = 0.5f;
    [SerializeField] private bool _useSmoothLoofanAnimation = false;
    
    public bool UseSmoothLoofanAnimation => _useSmoothLoofanAnimation;

    [Header("Cream Apply Animation")]
    [SerializeField] private RectTransform[] _creamApplyPositions;   
    [SerializeField] private float _creamApplyStepTime = 0.35f;

    private Sequence _returnSequence;
    private bool _isDragging;
    private bool _hasTool;
    private bool _inputBlocked = false;

    private MakeupData _selectedData;

    private Dictionary<MakeupTool, GameObject> _toolVisuals = new();

    private void Awake()
    {
        Instance = this;

        _toolVisuals = new Dictionary<MakeupTool, GameObject>
        {
            { MakeupTool.Cream,     _cream.gameObject },
            { MakeupTool.Loofan,    _loofan.gameObject },
            { MakeupTool.Blush,     _blushBrush.gameObject },
            { MakeupTool.Eyeshadow, _eyeBrush.gameObject }
        };
    }

    public void SetInputBlocked(bool value) => _inputBlocked = value;

    public bool HasTool => _hasTool; 

    public void TakeTool(MakeupData data, GameObject objToHide)
    {
        SetInputBlocked(true);
        _handRect.DOMove(objToHide.transform.position, 2f).OnComplete(() =>
        {
            StartDragging(data);
            if (_selectedData.type == MakeupTool.Lipstick)
            {
                MakeupPalette.Instance.ClosePalette();
            }
        });
    }

    public void TakeCream(MakeupData data, GameObject objToHide)
    {
        SetInputBlocked(true);
        _handRect.DOMove(objToHide.transform.position, 0.8f).OnComplete(() =>
        {
            StartDragging(data);
            _handRect.DOMove(_afterTakePosition.position, 0.7f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                MakeupPalette.Instance.ClosePalette();
                SetInputBlocked(false);
            });
        });
    }

    public void StartToolPickup(MakeupData data, GameObject colorVariant)
    {
        SetInputBlocked(true);
        FaceController.Instance.ResetMakeup(data);

        if (data.type == MakeupTool.Lipstick)
        {
            _handRect.DOMove(colorVariant.transform.position, 0.85f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                StartDragging(data);
                MoveToChestPosition();
            });
        }
        else if (data.type == MakeupTool.Eyeshadow || data.type == MakeupTool.Blush)
        {
            RectTransform brushPoint = (data.type == MakeupTool.Eyeshadow) 
                ? _eyeBrushPickupPoint : _blushBrushPickupPoint;

            _handRect.DOMove(brushPoint.position, 0.8f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                if (data.type == MakeupTool.Eyeshadow && _eyeBrushPaletteObject != null)
                    _eyeBrushPaletteObject.SetActive(false);
                else if (data.type == MakeupTool.Blush && _blushBrushPaletteObject != null)
                    _blushBrushPaletteObject.SetActive(false);

                StartDragging(data);

                _handRect.DOMove(colorVariant.transform.position, 0.75f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    PerformDippingAnimation(() => MoveToChestPosition());
                });
            });
        }
    }

    private void MoveToChestPosition()
    {
        _handRect.DOMove(_chestPosition.position, 0.85f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            if (_selectedData.type == MakeupTool.Lipstick)
            {
                MakeupPalette.Instance.ClosePalette();
            }
            SetInputBlocked(false);
        });
    }

    private void PerformDippingAnimation(Action onComplete)
    {
        Vector2 original = _handRect.position;
        Vector2 dipped = original + Vector2.down * _dippingDepth;

        Sequence seq = DOTween.Sequence();
        seq.Append(_handRect.DOMove(dipped, 0.22f).SetEase(Ease.InQuad));
        seq.Append(_handRect.DOMove(original, 0.28f).SetEase(Ease.OutQuad));
        seq.AppendInterval(_dippingPause);
        seq.OnComplete(() => onComplete?.Invoke());
    }

    public void StartDragging(MakeupData data)
    {
        _selectedData = data;
        _hasTool = true;
        _isDragging = false;                   

        ActivateToolVisual(data);

        if (data.type == MakeupTool.Loofan)
            StartLoofanAnimation();
    }

    public void FinishMakeupApplication()
    {
        SetInputBlocked(true);
        _isDragging = false;
        _hasTool = false;
        _selectedData = null;

        foreach (var visual in _toolVisuals.Values)
            if (visual != null) visual.SetActive(false);

        foreach (var lipstick in _lipsticks)
            if (lipstick != null) lipstick.gameObject.SetActive(false);

        ReturnToDefault();
    }

    public void FinishLoofan()
    {
        FaceController.Instance.Clear();
        SetInputBlocked(false);
        _hasTool = false;
        _selectedData = null;
        _isDragging = false;

        foreach (var visual in _toolVisuals.Values)
            if (visual != null) visual.SetActive(false);

        ReturnToDefault();
        MakeupParticleController.Instance.PlayLoofanParticles();
    }

    private void ActivateToolVisual(MakeupData data)
    {
        foreach (var visual in _toolVisuals.Values)
            if (visual != null) visual.SetActive(false);

        foreach (var lipstick in _lipsticks)
            if (lipstick != null) lipstick.gameObject.SetActive(false);

        if (data.type == MakeupTool.Lipstick && data.index < _lipsticks.Length)
        {
            _lipsticks[data.index].gameObject.SetActive(true);
        }
        else if (_toolVisuals.TryGetValue(data.type, out GameObject visual))
        {
            if (visual != null) visual.SetActive(true);
        }
    }

    public void UpdateHandPosition()
    {
        if (_inputBlocked || !_hasTool) return;

        if (Input.GetMouseButton(0))
        {
            _isDragging = true;
            _returnSequence?.Kill();

            Vector2 target = (Vector2)Input.mousePosition + _dragOffset;
            _handRect.position = Vector2.Lerp(_handRect.position, target, _dragSmooth * Time.deltaTime);
        }
        else
        {
            _isDragging = false;
        }
    }

    public bool IsDragging => _isDragging;

    public void StopDragging() => _isDragging = false;

    public void ReturnToDefault()
    {
        _blushBrushPaletteObject.SetActive(true);
        _eyeBrushPaletteObject.SetActive(true);
    
        _returnSequence?.Kill();
        _returnSequence = DOTween.Sequence();
        _returnSequence.Append(_handRect.DOAnchorPos(_defaultPos, 2f).SetEase(Ease.OutQuad));
    }
    
    public void ReturnBrushAndToDefault(float time = 2f) 
    {
        if (_selectedData == null) return;

        SetInputBlocked(true);
        _hasTool = false;
        _isDragging = false;

        MakeupTool tool = _selectedData.type;

        if (tool == MakeupTool.Blush || tool == MakeupTool.Eyeshadow)
        {
            RectTransform rt = (tool == MakeupTool.Blush) ? _blushBrushPickupPoint : _eyeBrushPickupPoint;

            Sequence seq = DOTween.Sequence();
            seq.Append(_handRect.DOMove(rt.position, time).SetEase(Ease.OutQuad));
            seq.AppendCallback(() =>
            {
                foreach (var visual in _toolVisuals.Values)
                    if (visual != null) visual.SetActive(false);

                if (tool == MakeupTool.Eyeshadow && _eyeBrushPaletteObject != null)
                    _eyeBrushPaletteObject.SetActive(true);
                else if (tool == MakeupTool.Blush && _blushBrushPaletteObject != null)
                    _blushBrushPaletteObject.SetActive(true);

                MakeupPalette.Instance.ClosePalette();

                ReturnToDefault();
            });
        }
        else
        {
            ReturnToDefault();
        }

        _selectedData = null;
    }

    public void StartCreamApplyAnimation()
    {
        SetInputBlocked(true);
        _isDragging = false;

        Sequence applySeq = DOTween.Sequence();
        foreach (var point in _creamApplyPositions)
        {
            applySeq.Append(_handRect.DOMove(point.position, _creamApplyStepTime).SetEase(Ease.InOutSine));
        }

        applySeq.OnComplete(() =>
        {
            FaceController.Instance.ApplyCream();
            MakeupParticleController.Instance.PlayApplyParticles();
            FinishMakeupApplication();
        });
    }

    private void StartLoofanAnimation()
    {
        SetInputBlocked(true);

        if (_useSmoothLoofanAnimation)
        {
            Sequence seq = DOTween.Sequence();
            foreach (var point in _loofanAnimPosition)
            {
                seq.Append(_handRect.DOMove(point.position, _stepTime).SetEase(Ease.InOutSine));
            }

            seq.OnComplete(() => FinishLoofan());
        }
        else
        {
            FinishLoofan();
        }
    }

    public MakeupData SelectedData => _selectedData;
}