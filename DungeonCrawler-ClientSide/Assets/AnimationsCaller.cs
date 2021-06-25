using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationsCaller : MonoBehaviour
{
	Animator anim;
	private void OnEnable()
	{
		UISELECTOR.SkillSelectedEvent += SetSkillType;
	}
	private void OnDisable()
	{
		UISELECTOR.SkillSelectedEvent -= SetSkillType;
	}
	public void SetSkillType(int type)
	{
		anim.SetInteger("AttackType", type);
	}
	private void Start()
	{
		anim = GetComponent<Animator>();
	}
	public void TriggerHurtAnimation()
	{
		anim.SetTrigger("Damaged");
	}
	public void ResetHurtTrigger()
	{
		anim.ResetTrigger("Damaged");
	}
	public void DoAttackAnimation()
	{
		anim.SetTrigger("Attack");
	}
	public void ResetAttackTrigger()
	{
		anim.ResetTrigger("Attack");
	}
	public void DoDeathAnimation()
	{
		anim.SetTrigger("Death");
	}
	public void DoHealAnimation()
	{
		anim.SetTrigger("Heal");
	}
	public void HealAnimationReset()
	{
		anim.ResetTrigger("Heal");
	}
	public void GetHealedAnimation()
	{
		anim.SetTrigger("GetHealed");
	}
	public void ResetHealedAnimation()
	{
		anim.ResetTrigger("GetHealed");
	}
}
