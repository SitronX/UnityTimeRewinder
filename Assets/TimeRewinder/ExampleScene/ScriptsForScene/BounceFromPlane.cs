using UnityEngine;

public class BounceFromPlane : MonoBehaviour
{
    [SerializeField] int jumpForce = 10;
    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rigidbody = other.GetComponent<Rigidbody>();
        rigidbody.AddForce(Vector3.up*jumpForce,ForceMode.Impulse);
        rigidbody.AddTorque(new Vector3(0.5f, 0.5f, 0), ForceMode.Impulse);

    }
}
