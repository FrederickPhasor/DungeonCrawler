using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorEvents : MonoBehaviour
{
    [SerializeField] GameObject ParticleEmisor;

    public void HealAnimations()
    {
        this.gameObject.GetComponent<Animator>().SetTrigger("Heal");
        this.gameObject.GetComponent<Animator>().ResetTrigger("Heal");
    }

    public void ActivateParticleEmision()
    {
        ParticleEmisor.gameObject.SetActive(true);
    }
    public void DeactivateParticleEmision()
    {
        ParticleEmisor.gameObject.SetActive(false);
    }
}
