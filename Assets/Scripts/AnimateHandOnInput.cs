using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimateHandOnInput : MonoBehaviour
{
    public InputActionProperty pinchAnimationAction;
    public InputActionProperty gripAnimationAction;

    public Animator handAnimator;

    // Update is called once per frame
    void Update()
    {
        float TriggerValue = pinchAnimationAction.action.ReadValue<float>();
        handAnimator.SetFloat("Trigger", TriggerValue);

        float GripValue = gripAnimationAction.action.ReadValue<float>();
        handAnimator.SetFloat("Grip", GripValue);

    }
}
