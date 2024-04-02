using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunBarell : MonoBehaviour
{
    [SerializeField] GameObject _projectilePrefab;
    [SerializeField] Transform _spawnPoint;

    public void Fire()
    {
        GameObject projectile = Instantiate(_projectilePrefab, _spawnPoint.position, Quaternion.identity);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        RewindAbstract rewindAbstract = projectile.GetComponent<RewindAbstract>();

        RewindManager.Instance.AddObjectForTracking(rewindAbstract, RewindManager.OutOfBoundsBehaviour.DisableDestroy);     //Attaching it to the tracked in mid game

        rb.AddForce((_spawnPoint.forward + new Vector3(Random.Range(0, 1), Random.Range(0, 1), Random.Range(0, 1)))*20, ForceMode.Impulse);
    }
}
