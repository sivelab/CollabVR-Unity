using UnityEngine;
using System;
using System.Collections;

[Serializable]
public struct IKData
{
    public AvatarIKGoal goal;
    public Transform target;
}

public class IKController : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private IKData[] goals;

    private void OnAnimatorIK()
    {
        foreach (var item in goals)
        {
            var goal = item.goal;
            var target = item.target;

            animator.SetIKPositionWeight(goal, 1);
            animator.SetIKRotationWeight(goal, 1);
            animator.SetIKPosition(goal, target.position);
            animator.SetIKRotation(goal, target.rotation);
        }
    }
}
