using UnityEngine;

public class MakeupApplier : MonoBehaviour
{
    public static MakeupApplier Instance;

    [Header("Face Zones")]
    [SerializeField] private RectTransform _faceZone;
    [SerializeField] private RectTransform _lipstickZone;
    [SerializeField] private RectTransform _blushZone;
    [SerializeField] private RectTransform _eyeZone;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!HandController.Instance.HasTool) return;

        var selectedData = HandController.Instance.SelectedData;
        if (selectedData == null) return;

        HandController.Instance.UpdateHandPosition();

        if (Input.GetMouseButton(0) &&
            (selectedData.type == MakeupTool.Lipstick && IsOverZone(_lipstickZone) ||
             selectedData.type == MakeupTool.Eyeshadow && IsOverZone(_eyeZone) ||
             selectedData.type == MakeupTool.Blush && IsOverZone(_blushZone)))
        {
            FaceController.Instance.ApplyMakeupProgress(selectedData, Time.deltaTime, selectedData.type);
        }

        if (Input.GetMouseButtonUp(0))
        {
            HandController.Instance.StopDragging();

            if (IsOverFace())
                ApplyTool();
            else
                HandController.Instance.ReturnToDefault();
        }
    }

    private bool IsOverFace() =>
        RectTransformUtility.RectangleContainsScreenPoint(_faceZone, Input.mousePosition, null);

    private bool IsOverZone(RectTransform zone) =>
        RectTransformUtility.RectangleContainsScreenPoint(zone, Input.mousePosition, null);

    private void ApplyTool()
    {
        var data = HandController.Instance.SelectedData;
        if (data == null) return;

        if (data.type == MakeupTool.Cream)
        {
            HandController.Instance.StartCreamApplyAnimation();
        }
        else
        {
            HandController.Instance.FinishMakeupApplication();
        }
    }

    public void SelectTool(MakeupData data, GameObject objToHide)
    {
        FaceController.Instance.ResetMakeup(data);
        HandController.Instance.TakeTool(data, objToHide);
    }
}