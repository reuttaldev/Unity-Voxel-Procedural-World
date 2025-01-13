using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DigAndBuildMechanism : MonoBehaviour
{
    [SerializeField]
    private Transform camera, digHighlight,buildHighlight;
    [SerializeField]
    private InputActionReference digAction, buildAction, scrollAction, onOffAction;
    [SerializeField]
    private float raycastLength; // the maximum distance between us and a voxel we are allowed to dig 
    [SerializeField]
    private LayerMask chunkLayer;
    [SerializeField]
    // some VoxelTypes are not ones that can be built
    private VoxelType[] buildableVoxelTypes;
    [SerializeField]
    TMP_Text voxelTypeText;

    private int selectedVoxelIndex = 1;
    private int voxelOptionsLength=1;
    private Ray ray;
    // gloval position
    private Vector3Int rayEndPoint;
    bool rayCollision = true, on =false;
    private void OnEnable()
    {
        buildAction.action.performed += Build;
        digAction.action.performed += Dig;
        scrollAction.action.performed += ControlSlectedVoxelType;
        onOffAction.action.performed += ControlState;

    }
    private void OnDisable()
    {
        buildAction.action.performed -= Build;
        digAction.action.performed -= Dig;
        scrollAction.action.performed -= ControlSlectedVoxelType;
        onOffAction.action.performed -= ControlState;
    }
    private void Awake()
    {
        voxelOptionsLength = buildableVoxelTypes.Length;
        UpdateText();
    }

    private void Start()
    {
        on = false;
        ControlVisuals();
    }

    void ControlState(InputAction.CallbackContext callback)
    {
        on = !on;
        ControlVisuals();
    }
    void ControlVisuals()
    {
        if (!on)
        {
            voxelTypeText.gameObject.SetActive(false);
            digHighlight.gameObject.SetActive(false);
            buildHighlight.gameObject.SetActive(false);

        }
        else
        {
            voxelTypeText.gameObject.SetActive(true);
            digHighlight.gameObject.SetActive(true);
            buildHighlight.gameObject.SetActive(true);
        }

    }
    private void ControlSlectedVoxelType(InputAction.CallbackContext callback)
    {
        var val = scrollAction.action.ReadValue<float>();
        if(val > 0)
            selectedVoxelIndex = (selectedVoxelIndex -1 + voxelOptionsLength) % voxelOptionsLength;
        else
            selectedVoxelIndex = (selectedVoxelIndex +1 ) % voxelOptionsLength;
        UpdateText ();
    }

    private void UpdateText()
    {
        var type = buildableVoxelTypes[selectedVoxelIndex];
        voxelTypeText.text = type.ToString();
    }

    // check if there is a voxel in front of us 
    private bool CheckRayCollision()
    {
        return Physics.Raycast(ray, out var hit, raycastLength, chunkLayer);
    }
    private void ReachToFront()
    {
        ray = new Ray(camera.transform.position, camera.transform.forward);
        // fill values for the dig and build methods
        rayEndPoint = Vector3Int.RoundToInt(ray.GetPoint(raycastLength));
        rayCollision = CheckRayCollision();
        buildHighlight.position = rayEndPoint;
    }
    private void Update()
    {
        if (!on)
            return;

        ReachToFront();
#if UNITY_EDITOR
        Debug.DrawLine(ray.origin, rayEndPoint, Color.red);
#endif
    }
    private void Build(InputAction.CallbackContext callback)
    {
        if (!on)
            return;
        var type = buildableVoxelTypes[selectedVoxelIndex];
        ChunkContoller.Instance.SetVoxelTypeByGlobalPos(rayEndPoint, type, true);
    }
    private void Dig(InputAction.CallbackContext callback)
    {
        if (!on)
            return;
        if (rayCollision)
        {
            ChunkContoller.Instance.SetVoxelTypeByGlobalPos(rayEndPoint, VoxelType.Empty,true);
            Debug.Log("digging");
        }
    }

}
