using UnityEngine;

public class MakeupPalette : MonoBehaviour
{
    public static MakeupPalette Instance;

    [Header("Panel")]
    [SerializeField] private GameObject _palettePanel;

    [Header("Eye")]
    [SerializeField] private GameObject _eyeshadowLayout;
    [SerializeField] private GameObject _eyeBrushParent;
    
    [Header("Lipstick")]
    [SerializeField] private GameObject _lipstickLayout;
    
    [Header("Blush")]
    [SerializeField] private GameObject _blushLayout;
    [SerializeField] private GameObject _blushBrushParent;

    private MakeupCategoryButton _currentActiveButton = null; 

    private void Awake()
    {
        Instance = this;
        _palettePanel.SetActive(false);
        HideAllLayouts();
    }

    private void HideAllLayouts()
    {
        if (_eyeshadowLayout) _eyeshadowLayout.SetActive(false);
        if (_lipstickLayout) _lipstickLayout.SetActive(false);
        if (_blushLayout) _blushLayout.SetActive(false);
        _eyeBrushParent.SetActive(false);
        _blushBrushParent.SetActive(false);
    }

    public void OpenPalette(MakeupTool type, MakeupCategoryButton callerButton)
    {
        HandController.Instance.SetInputBlocked(true); 
        if (_currentActiveButton == callerButton)
        {
            _palettePanel.SetActive(true);
            return;
        }

        if (_currentActiveButton != null)
        {
            _currentActiveButton.ResetToOriginal();
        }

        _currentActiveButton = callerButton;

        HideAllLayouts();

        switch (type)
        {
            case MakeupTool.Eyeshadow:
                if (_eyeshadowLayout)
                {
                    _eyeshadowLayout.SetActive(true);
                    _eyeBrushParent.SetActive(true);
                }
                break;
            case MakeupTool.Lipstick:
                if (_lipstickLayout) _lipstickLayout.SetActive(true);
                break;
            case MakeupTool.Blush:
                if (_blushLayout)
                {
                    _blushLayout.SetActive(true);
                    _blushBrushParent.SetActive(true);
                }
                break;
        }

        _palettePanel.SetActive(true);
    }

    public void ClosePalette()
    {
        HandController.Instance.SetInputBlocked(false);
        _currentActiveButton?.ResetToOriginal();
        _palettePanel.SetActive(false);
    }
}