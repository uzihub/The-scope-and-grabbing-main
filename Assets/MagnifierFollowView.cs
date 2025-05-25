using UnityEngine;

public class MagnifierFollowView : MonoBehaviour
{
    public Transform lensSurface;
    public Transform viewerCamera;
    public float magnifierOffset = 0.02f;
    public float customFOV = 20f;

    private Camera magnifierCam;

    void Start()
    {
        magnifierCam = GetComponent<Camera>();
        if (magnifierCam != null)
        {
            magnifierCam.fieldOfView = customFOV;
        }
    }

    void LateUpdate()
    {
        if (lensSurface == null || viewerCamera == null || magnifierCam == null) return;

        // Get world space ray direction: from viewer eye TO lens center
        Vector3 viewToLens = (lensSurface.position - viewerCamera.position).normalized;

        // Set camera at lens center, slightly behind (to avoid clipping)
        transform.position = lensSurface.position - viewToLens * magnifierOffset;

        // Make magnifier camera look along that vector
        transform.rotation = Quaternion.LookRotation(viewToLens, viewerCamera.up);
    }
}
