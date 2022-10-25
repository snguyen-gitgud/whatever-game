using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStatBehavior : MonoBehaviour
{
    [Header("Stats")]
    public int healthPoint = 10;
    public int defensePoint = 0;

    [Header("Rendering and Animating")]
    public Renderer renderer;
    public Material whiteMaterial;
    public LeanTweenType easeType;
    public Animator animator;

    [Header("Effects")]
    public Vector3 hitShakingEffectMagnitude = new Vector3(0.01f, 0.0f, 0.0f);
    public GameObject hitVFX;

    //internal
    Material baseMaterial;
    Coroutine hitCoroutine = null;
    Guirao.UltimateTextDamage.UltimateTextDamageManager textManager;

    private void Start()
    {
        baseMaterial = renderer.material;
        textManager = CommonRefManager.Instance.textDamageCanvas.GetComponent<Guirao.UltimateTextDamage.UltimateTextDamageManager>();
    }

    public void GetHit(int damage, Vector3 impact_pos)
    {
        //Debug.Log("hit: " + damage);

        damage -= defensePoint;
        if (damage < 0)
            damage = 0;

        if (hitCoroutine == null)
            hitCoroutine = StartCoroutine(DelayTurnOffInvincible(damage, impact_pos));
        else
        {
            StopCoroutine(hitCoroutine);
            hitCoroutine = StartCoroutine(DelayTurnOffInvincible(damage, impact_pos));
        }

        healthPoint -= damage;
        if (healthPoint <= 0)
        {
            StartCoroutine(DeathSequence());
        }
    }

    IEnumerator DelayTurnOffInvincible(int damage, Vector3 impact_pos)
    {
        if (textManager == null)
            textManager = CommonRefManager.Instance.textDamageCanvas.GetComponent<Guirao.UltimateTextDamage.UltimateTextDamageManager>();
        textManager.Add(damage.ToString(), this.transform);

        Instantiate(hitVFX, impact_pos, this.transform.rotation);

        Time.timeScale = 0.01f;
        renderer.material = whiteMaterial;
        LeanTween.move(renderer.gameObject, renderer.transform.position + hitShakingEffectMagnitude * damage, 0.05f).setLoopPingPong(2).setEase(easeType);
        yield return new WaitForSecondsRealtime(0.1f + damage * 0.025f);
        renderer.material = baseMaterial;
        Time.timeScale = 1.0f;
    }

    IEnumerator DeathSequence()
    {
        this.GetComponent<Collider>().enabled = false;
        //renderer.transform.parent.localPosition += Vector3.forward;

        if (animator != null)
        {
            //TODO: trigger death animation
        }
        else
        {
            LeanTween.rotateX(renderer.gameObject, -60.0f, 0.25f);
        }

        if (this.GetComponent<BaseAIBehavior>() != null)
            this.GetComponent<BaseAIBehavior>().StopAI();

        yield return new WaitForSecondsRealtime(5.0f);
        LeanTween.move(renderer.gameObject, this.transform.position - Vector3.up * 2.0f, 1.0f);
        yield return new WaitForSecondsRealtime(1.25f);
        //TODO: remove object here
        LeanTween.cancel(this.gameObject);
        Destroy(this.gameObject);
    }
}
