using UnityEngine;
using UnityEngine.EventSystems;

public class MakeupVariant : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private MakeupData _data;

    public void OnPointerClick(PointerEventData _eventData)
    {
        if (_data.type == MakeupTool.Cream)
        {
            HandController.Instance.TakeCream(_data, gameObject);
        }
        else if (_data.type == MakeupTool.Loofan)
        {
            if (HandController.Instance.UseSmoothLoofanAnimation)
            {
                HandController.Instance.TakeTool(_data, gameObject);
            }
            else
            {
                HandController.Instance.FinishLoofan();
            }
        }
        else
        {
            HandController.Instance.StartToolPickup(_data, gameObject);
        }
    }
}