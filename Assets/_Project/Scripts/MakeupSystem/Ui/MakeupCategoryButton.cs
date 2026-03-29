using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MakeupCategoryButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private MakeupTool _toolType;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Sprite _onPressImage;

    private Sprite _originalSprite;

    private bool _isActiveCategory = false;

    private void Awake()
    {
        if (_iconImage != null)
        {
            _originalSprite = _iconImage.sprite;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        MakeupPalette.Instance.OpenPalette(_toolType, this);
        
        _iconImage.sprite = _onPressImage;

        _isActiveCategory = true;
    }
    public void ResetToOriginal()
    {
        if (_iconImage == null) return;

        _iconImage.sprite = _originalSprite;

        _isActiveCategory = false;
    }

    public MakeupTool ToolType => _toolType;
    public bool IsActiveCategory => _isActiveCategory;
}