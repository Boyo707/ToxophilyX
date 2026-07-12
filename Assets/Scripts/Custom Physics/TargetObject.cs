using UnityEngine;
using CustomPhysics;
using System.Collections;


public class TargetObject : MonoBehaviour
{
    [SerializeField] private GameObject breakParticles;
    [SerializeField] private ParticleSystem winParticle;
    [SerializeField] private Animator anim;
    [SerializeField] private PlayerControlls player;
    PhysicsObject objPhysics;

    private bool isHit = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        objPhysics = GetComponent<PhysicsObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (objPhysics.hasTriggered && !isHit)
        {
            StartCoroutine(PerformWin());
        }
    }

    private IEnumerator PerformWin()
    {
        isHit = true;
        objPhysics.interactedObject.SetVelocity(new Vector3(Random.Range(-5, 5), Random.Range(-5, 5), 0));
        Instantiate(breakParticles, transform.position, Quaternion.identity);
        winParticle.Play();
        GetComponent<SpriteRenderer>().enabled = false;
        player.enabled = false;
        yield return new WaitForSeconds(1.5f);
        objPhysics.isGettingDestroyed = true;
        anim.SetTrigger("showButton");
    }
}
