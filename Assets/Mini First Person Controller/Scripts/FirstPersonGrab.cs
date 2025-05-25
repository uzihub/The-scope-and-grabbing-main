using UnityEngine;

public class FirstPersonGrab : MonoBehaviour
{
    [Header("Hold Settings")]
    public Transform playerCamera;
    public GameObject rightHand;
    public GameObject leftHand;
    public Rigidbody playerRigidbody; // Reference to player's Rigidbody

    [Header("Magnifying Glass Settings")]
    public float holdDistance = 1.0f;
    public float lateralOffset = 0.3f;
    public Vector3 grabOffset = new Vector3(0, -0.1f, 0);
    public float minHeightOffset = 0.5f;
    public float twoHandHeightOffset = 0.2f; // Offset when using both hands
    public Camera magnifyCamera;
    public RenderTexture magnifyRenderTexture;
    public GameObject magnifyingGlassGlass;

    [Header("Throw Settings")]
    public float throwForce = 10f;

    private Rigidbody grabbedObject;
    private Collider objectToGrab;
    private bool isHolding = false;
    private bool holdingInRightHand = true;
    private bool isHoldingWithBothHands = false; // Toggle for two-hand hold
    private Quaternion initialRotation; // Store the initial rotation of the magnifying glass

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) // Press E to grab/drop
        {
            if (grabbedObject == null)
            {
                TryGrabObject();
            }
            else
            {
                DropObject();
            }
        }

        if (isHolding && grabbedObject != null)
        {
            if (Input.GetKeyDown(KeyCode.Q)) // Press Q to toggle two-handed hold
            {
                isHoldingWithBothHands = !isHoldingWithBothHands;
            }

            if (isHoldingWithBothHands)
            {
                // Get midpoint between both hands
                Vector3 midpoint = (rightHand.transform.position + leftHand.transform.position) / 2;

                // Move the magnifying glass slightly higher when using both hands
                midpoint.y += twoHandHeightOffset;

                // Apply the grab offset to the midpoint
                grabbedObject.transform.position = midpoint + grabOffset;

                // Align the magnifying glass with the player's view
                grabbedObject.transform.rotation = Quaternion.LookRotation(playerCamera.forward, Vector3.up);
            }
            else
            {
                // Adjust position based on active hand
                Vector3 lateralShift = playerCamera.right * (holdingInRightHand ? lateralOffset : -lateralOffset);
                Vector3 targetPosition = playerCamera.position + playerCamera.forward * holdDistance + grabOffset + lateralShift;
                targetPosition.y = Mathf.Max(targetPosition.y, minHeightOffset);
                grabbedObject.transform.position = targetPosition;
                grabbedObject.transform.rotation = Quaternion.LookRotation(playerCamera.forward, Vector3.up);
            }

            // Magnify Camera follows lens correctly
            if (magnifyCamera != null)
            {
                Vector3 lensOffset = new Vector3(0, -0.1f, 0);
                magnifyCamera.transform.position = grabbedObject.transform.position + lensOffset;
                magnifyCamera.transform.rotation = playerCamera.transform.rotation;
            }
        }

        // Switch hands using Tab when NOT holding with two hands
        if (Input.GetKeyDown(KeyCode.Tab) && isHolding && !isHoldingWithBothHands)
        {
            holdingInRightHand = !holdingInRightHand;
        }

        if (Input.GetMouseButtonDown(1) && grabbedObject != null) // Right-click to throw
        {
            ThrowObject();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grabbable"))
        {
            objectToGrab = other;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == objectToGrab)
        {
            objectToGrab = null;
        }
    }

    void TryGrabObject()
    {
        if (objectToGrab == null) return;

        grabbedObject = objectToGrab.GetComponentInParent<Rigidbody>();

        if (grabbedObject == null) return;

        grabbedObject.useGravity = false;
        grabbedObject.isKinematic = true;
        grabbedObject.transform.SetParent(null);

        // Store the initial rotation of the magnifying glass
        initialRotation = grabbedObject.transform.rotation;

        isHolding = true;
        holdingInRightHand = true;
        isHoldingWithBothHands = false;

        // Enable Magnify Camera
        if (magnifyCamera != null)
        {
            magnifyCamera.enabled = true;
        }

        // Apply zoom texture to the lens
        if (magnifyingGlassGlass != null)
        {
            Material glassMaterial = magnifyingGlassGlass.GetComponent<MeshRenderer>().material;
            if (glassMaterial != null)
            {
                glassMaterial.mainTexture = magnifyRenderTexture;
            }
        }
    }

    void DropObject()
    {
        if (grabbedObject == null) return;

        isHolding = false;
        isHoldingWithBothHands = false;

        grabbedObject.useGravity = true;
        grabbedObject.isKinematic = false;
        grabbedObject = null;

        // Disable Magnify Camera
        if (magnifyCamera != null)
        {
            magnifyCamera.enabled = false;
        }

        // Remove zoom effect from the lens
        if (magnifyingGlassGlass != null)
        {
            magnifyingGlassGlass.GetComponent<MeshRenderer>().material.mainTexture = null;
        }
    }

    void ThrowObject()
    {
        if (grabbedObject == null) return;

        isHolding = false;
        isHoldingWithBothHands = false;

        grabbedObject.useGravity = true;
        grabbedObject.isKinematic = false;
        grabbedObject.AddForce(playerCamera.forward * throwForce, ForceMode.Impulse);
        grabbedObject = null;

        // Disable Magnify Camera
        if (magnifyCamera != null)
        {
            magnifyCamera.enabled = false;
        }

        // Remove zoom effect from the lens
        if (magnifyingGlassGlass != null)
        {
            magnifyingGlassGlass.GetComponent<MeshRenderer>().material.mainTexture = null;
        }
    }
}
