using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This script detects a collision with another object (collider) which has a predefined tag. On collision, the "OnObjectTouch" event will be fired.
/// </summary>
public class ObjectCollision : MonoBehaviour
{
    [SerializeField] private string colliderTag;
    [SerializeField] private UnityEvent onObjectTouch;
    [SerializeField] private float collisionVelocity = 0.1f;

    private bool coolingDown = false;
    private WaitForSeconds coolDownTime = new WaitForSeconds(0.2f);

    private void OnCollisionEnter(Collision collision)
    {
        if (!coolingDown && collision.gameObject.CompareTag(colliderTag) && collision.relativeVelocity.magnitude > collisionVelocity)
        {
            // The cool down prevents triggering immediately one after another.
            coolingDown = true;
            StartCoroutine(CollisionCooldown());
            onObjectTouch.Invoke();
        }
    }

    private IEnumerator CollisionCooldown()
    {
        yield return coolDownTime;
        coolingDown = false;
    }

    private void OnEnable()
    {
        coolingDown = false;
    }
}
