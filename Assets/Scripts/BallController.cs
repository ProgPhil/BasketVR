using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class BallController : MonoBehaviour
{
    public float bounceForce = 5f;
    private Rigidbody rb;

    private Transform grabbedBy = null;
    private bool touchesGround;
    private string lastHandTouched = "";

    public LineRenderer throwLine;
    public float previewLength = 5f;

    public InputActionProperty rightThrow;
    public InputActionProperty leftThrow;
    public InputActionProperty rightGrab;
    public InputActionProperty leftGrab;
    public Transform leftThrowAnchor;
    public Transform rightThrowAnchor;
    private bool isRightThrowing;
    private bool isLeftThrowing;
    private HashSet<Transform> rightHandContacts = new HashSet<Transform>();
    private HashSet<Transform> leftHandContacts = new HashSet<Transform>();

    public AudioSource bounceSound;
    public AudioSource catchSound;

    private int groundBounceCount = 0;
    public int maxBounceCount = 5;
    public float initialGroundBounceForce = 7f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        touchesGround = false;
    }

    void Update()
    {
        float rightThrowStrenght = rightThrow.action.ReadValue<float>();
        float leftThrowStrenght = leftThrow.action.ReadValue<float>();
        float rightGrabStrenght = rightGrab.action.ReadValue<float>();
        float leftGrabStrenght = leftGrab.action.ReadValue<float>();
        if (isRightThrowing && rightThrowStrenght == 0 && rightGrabStrenght == 0)
        {
            rb.AddForce(-rightThrowAnchor.forward * 10, ForceMode.Impulse);
            isRightThrowing = false;
            isLeftThrowing = false;
            throwLine.enabled = false;
        }
        if (isLeftThrowing && leftThrowStrenght == 0 && leftGrabStrenght == 0)
        {
            rb.AddForce(-leftThrowAnchor.forward * 10, ForceMode.Impulse);
            isRightThrowing = false;
            isLeftThrowing = false;
            throwLine.enabled = false;
        }
        if (isRightThrowing && (rightThrowStrenght > 0 || rightGrabStrenght > 0))
        {
            transform.position = rightThrowAnchor.position;
            Vector3 start = rightThrowAnchor.position;
            Vector3 end = start + (rightThrowAnchor.right * previewLength);

            throwLine.enabled = true;
            throwLine.SetPosition(0, start);
            throwLine.SetPosition(1, end);
        }
        if (isLeftThrowing && (leftThrowStrenght > 0 || leftGrabStrenght > 0))
        {
            transform.position = leftThrowAnchor.position;
            Vector3 start = leftThrowAnchor.position;
            Vector3 end = start + (-leftThrowAnchor.right * previewLength);

            throwLine.enabled = true;
            throwLine.SetPosition(0, start);
            throwLine.SetPosition(1, end);
        }

        if (rightHandContacts.Count >= 1 || leftHandContacts.Count >= 1)
        {
            groundBounceCount = 0;
        }
        if (rightHandContacts.Count >= 1 && leftHandContacts.Count >= 1 && !isRightThrowing && !isLeftThrowing)
        {
            groundBounceCount = 0;
            Vector3 averagePos = Vector3.zero;
            int totalPoints = 0;

            foreach (var t in rightHandContacts)
            {
                averagePos += t.position;
                totalPoints++;
            }

            foreach (var t in leftHandContacts)
            {
                averagePos += t.position;
                totalPoints++;
            }

            if (totalPoints > 0)
            {
                averagePos /= totalPoints;
                Vector3 offsetPos = averagePos + Vector3.up * 0.1f;
                transform.position = Vector3.Lerp(transform.position, offsetPos, Time.deltaTime * 15f);
                rb.velocity = Vector3.zero;
            }
            if ((rightThrowStrenght > 0 || rightGrabStrenght > 0) && !isLeftThrowing && !isRightThrowing)
            {
                isRightThrowing = true;
            }
            if ((leftThrowStrenght > 0 || leftGrabStrenght > 0) && !isLeftThrowing && !isRightThrowing)
            {
                isLeftThrowing = true;
            }
        }
        if (transform.position.y > 7f)
        {
            rb.velocity = Vector3.zero;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider == null || collision.collider.attachedRigidbody != null && collision.collider.attachedRigidbody.gameObject != gameObject)
            return;
        if (collision.gameObject.CompareTag("RightHand"))
        {
            rightHandContacts.Add(collision.transform);
            HandleTouch(collision.transform, "RightHand");
        }
        else if (collision.gameObject.CompareTag("LeftHand"))
        {
            leftHandContacts.Add(collision.transform);
            HandleTouch(collision.transform, "LeftHand");
        }
        if (collision.gameObject.CompareTag("Ground"))
        {
            HandleGroundBounce();
            touchesGround = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider == null || collision.collider.attachedRigidbody != null && collision.collider.attachedRigidbody.gameObject != gameObject)
            return;
        if (collision.gameObject.CompareTag("RightHand"))
        {
            rightHandContacts.Remove(collision.transform);
            CheckRelease();
        }
        else if (collision.gameObject.CompareTag("LeftHand"))
        {
            leftHandContacts.Remove(collision.transform);
            CheckRelease();
        }
        if (collision.gameObject.CompareTag("Ground"))
        {
            touchesGround = false;
        }
    }

    void HandleGroundBounce()
    {
        if (groundBounceCount >= maxBounceCount) return;

        float bounceStrength = initialGroundBounceForce * (1f - ((float)groundBounceCount / maxBounceCount));
        rb.AddForce(Vector3.up * bounceStrength, ForceMode.Impulse);

        if (bounceSound != null)
        {
            bounceSound.Play();
        }

        groundBounceCount++;
    }


    void HandleTouch(Transform handTransform, string handName)
    {
        bool rightTouching = rightHandContacts.Count > 0;
        bool leftTouching = leftHandContacts.Count > 0;

        if (rightTouching && leftTouching)
        {
            if (lastHandTouched != handName)
            {
                grabbedBy = handTransform;
                lastHandTouched = handName;
            }
        }
        else if (!grabbedBy) // only bounce if not currently grabbed
        {
            if (touchesGround == true)
            {
                rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
            }
            else
            {
                Vector3 bounceDirection = (transform.position - handTransform.position).normalized;
                rb.AddForce(bounceDirection * 1f, ForceMode.Impulse);
                if (catchSound != null)
                {
                    catchSound.Play();
                }
            }

        }
    }

    void CheckRelease()
    {
        if (rightHandContacts.Count == 0 && leftHandContacts.Count == 0)
        {
            grabbedBy = null;
            lastHandTouched = "";
        }
    }
}
