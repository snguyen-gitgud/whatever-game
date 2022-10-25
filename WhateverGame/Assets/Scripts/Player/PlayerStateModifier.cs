using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerAnimationController))]
public class PlayerStateModifier : MonoBehaviour
{
    [Header("Controller")]
    public PlayerController controller = null;

    [Header("Stats")]
    public int healthPoint = 3;
    public int armorPoint = 0;
    public float invincibleDuration = 0.1f;

    [Header("Effect")]
    public Renderer playerRenderer = null;
    public GameObject playerDeathVfx = null;
    public GameObject hitVfx;
    public Material redMaterial;

    //internal
    Material baseMaterial;
    Coroutine invincibleCoroutine = null;
    PlayerAnimationController animationController;
    Guirao.UltimateTextDamage.UltimateTextDamageManager textManager;

    private void Start()
    {
        if (controller == null)
            controller = this.GetComponent<PlayerController>();

        baseMaterial = playerRenderer.material;
        animationController = this.GetComponent<PlayerAnimationController>();
        textManager = CommonRefManager.Instance.textDamageCanvas.GetComponent<Guirao.UltimateTextDamage.UltimateTextDamageManager>();
    }

    public void OnInstaDeath()
    {
        if (controller.m_IsActive == true)
        {
            controller.m_IsActive = false;
            Instantiate(playerDeathVfx, this.transform.position, this.transform.rotation);
            playerRenderer.enabled = false;
            GameMasterBehavior.Instance.OnGameOver();
        }   
    }

    public void GetHit(int damage, Vector3 contact_point)
    {
        if (invincibleCoroutine == null)
        {
            damage -= armorPoint;
            if (damage < 0)
                damage = 0;

            healthPoint -= damage;

            if (textManager == null)
                textManager = CommonRefManager.Instance.textDamageCanvas.GetComponent<Guirao.UltimateTextDamage.UltimateTextDamageManager>();
            textManager.Add("<color=red>" + damage.ToString() + "</color>", this.transform);

            //TODO: hit vfx
            Instantiate(hitVfx, contact_point, this.transform.rotation);
            invincibleCoroutine = StartCoroutine(HitBlinking(damage));

            if (animationController == null)
                animationController = this.GetComponent<PlayerAnimationController>();
            animationController.GetKnockBack();

            if (healthPoint <= 0)
            {
                if (controller.m_IsActive == true)
                {
                    controller.m_IsActive = false;
                    Instantiate(playerDeathVfx, this.transform.position, this.transform.rotation);
                    playerRenderer.enabled = false;
                    GameMasterBehavior.Instance.OnGameOver();
                }
            }
        }
    }

    IEnumerator HitBlinking(int damage)
    {
        //TODO: add controller shake


        playerRenderer.material = redMaterial;
        Time.timeScale = 0.0f;
        CommonRefManager.Instance.mainVCam.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0.5f;
        yield return new WaitForSecondsRealtime(invincibleDuration + damage * 0.025f);
        playerRenderer.material = baseMaterial;
        Time.timeScale = 1.0f;
        invincibleCoroutine = null;
        yield return new WaitForSecondsRealtime(0.33f);
        CommonRefManager.Instance.mainVCam.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0.0f;
    }
}
