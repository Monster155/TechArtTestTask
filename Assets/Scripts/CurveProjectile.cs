using System;
using System.Collections;
using UnityEngine;

public class CurveProjectile : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particle;
    [SerializeField] private float _flyTime = 1f;

    private void Start()
    {
        _particle.Stop();
    }

    public void Shoot(Vector3 startPos, Transform target)
    {
        StartCoroutine(ShootCoroutine(startPos, target));
    }

    private IEnumerator ShootCoroutine(Vector3 startPos, Transform target)
    {
        _particle.Play(true);

        float timer = 0;
        while (timer < _flyTime)
        {
            transform.position = Vector3.LerpUnclamped(startPos, target.position, timer / _flyTime);
            yield return null;
            timer += Time.deltaTime;
        }
        
        _particle.Stop(true);
    }
}