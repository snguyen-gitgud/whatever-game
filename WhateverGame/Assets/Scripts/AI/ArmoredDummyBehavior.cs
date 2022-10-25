using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmoredDummyBehavior : BaseAIBehavior
{
    [Header("Animation stuffs")]
    public GameObject model;
    public float normalYRotation = -110.0f;
    public float attackingYRotation = -180.0f;
    public float attackReadyYRotation = -60.0f;
    public GameObject attackVFX;
    public GameObject weapon;
    public Transform vfxParent;

    [Header("Combat")]
    public int attackPoint = 1;
    public float attackCooldownMin = 7.0f;
    public float attackCooldownMax = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(AttackingSequence());
        weapon.SetActive(false);
    }

    IEnumerator AttackingSequence()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(attackCooldownMin, attackCooldownMax));
            LeanTween.rotateY(model, attackReadyYRotation, 1.0f).setOnComplete(() => {
                weapon.SetActive(true);
                LeanTween.rotateY(model, attackingYRotation, 0.025f).setOnComplete(() => {
                    GameObject effect = Instantiate(attackVFX, vfxParent.position, vfxParent.rotation);
                    LeanTween.rotateY(model, normalYRotation, 1.0f).setDelay(2.0f).setOnComplete(() => {
                        weapon.SetActive(false);
                    });
                });
            });
        }
    }

    public override void StopAI()
    {
        StopAllCoroutines();
        LeanTween.cancel(model);
    }
}
