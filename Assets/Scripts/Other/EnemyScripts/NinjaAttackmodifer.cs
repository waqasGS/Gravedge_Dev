using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NinjaAttackmodifer : MonoBehaviour
{
    public int attackValue;
    public Animator animator;
    public void ChangeAttackAnimation()
    {
        attackValue = (attackValue + Random.Range(1, 3)) % 3;

        animator.SetInteger("AttackNumber", attackValue);
        Debug.Log("AttackChange: " + attackValue);
        Debug.Log("AttackChange");
    }
}
