using System.Collections;
using System.Collections.Generic;
using Invector;
using UnityEngine;

public class TitanAnimationController : MonoBehaviour
{
    public Animator _animator;
    public float delayStunned;
    //public Animator leftWing;
    //public Animator rightWing;
    public ParticleSystem hitParticles;
    public StunnedTimer shockedTimer;

    public void TakeDamage(vDamage _damage)
    {
        if (GetComponent<vHealthController>().currentHealth <= 0)
            return;

        _animator.SetInteger("ReactionID", _damage.reaction_id);
        if (hitParticles == null)
            return;
        hitParticles.Play();
        if (_damage.reaction_id == 0)
        {
            _animator.SetTrigger("NormalAttack");
        }
        _animator.SetTrigger("TriggerReaction");
        if (_damage.reaction_id == 3)
        {
            Invoke(nameof(DelayStunned), 0.2f);
        }
    }
    public void DelayStunned()
    {
        shockedTimer.StartStunned();
        //_animator.SetBool("AfterStunned", true);
        ////leftWing.enabled = false;
        ////rightWing.enabled = false;
        //Invoke(nameof(StunnedTimeOff), delayStunned);
    }
    public void StunnedTimeOff()
    {
        //leftWing.enabled = true;
        //rightWing.enabled = true;
        _animator.SetBool("AfterStunned", false);
    }

}
