using System;
using System.Collections;
using Spine;
using Spine.Unity;
using UnityEngine;
using Event = Spine.Event;

public class DragonAnimationControl : MonoBehaviour
{
    private float TILE_SIZE = 0.6f;

    [Header("Front dragon")]
    [SerializeField] private SkeletonAnimation _frontAnimator;
    [SerializeField] private AnimationReferenceAsset _frontIdleAnim;
    [SerializeField] private AnimationReferenceAsset _frontAttackAnim;
    [SerializeField] private AnimationReferenceAsset _frontAttack2Anim;
    [Space] [Header("Front dragon")]
    [SerializeField] private SkeletonAnimation _backAnimator;
    [SerializeField] private AnimationReferenceAsset _backIdleAnim;
    [SerializeField] private AnimationReferenceAsset _backAttackAnim;
    [SerializeField] private AnimationReferenceAsset _backAttack2Anim;
    [Space] [Header("Effects")]
    [SerializeField] private CurveProjectile _curveProjectile;
    [SerializeField] private ParticleSystem _attack2System;
    [Space] [Header("Other")]
    [SerializeField] private Transform _muzzle;
    [SerializeField] private Transform _target;

    private AnimType _currentAnim;
    private bool _isFrontView;
    private bool _isFrontViewWasLast;

    private void Start()
    {
        _frontAnimator.AnimationState.Event += FrontAnimator_Event;
        _backAnimator.AnimationState.Event += BackAnimator_Event;
        StartCoroutine(AttackAnimCoroutine());
        _isFrontView = _target.position.y < transform.position.y;
        _isFrontViewWasLast = !_isFrontView;
    }

    private void Update()
    {
        _isFrontView = _target.position.y < transform.position.y;

        UpdateFacing();
    }

    private void UpdateFacing()
    {
        if (_isFrontViewWasLast == _isFrontView)
            return;

        _isFrontViewWasLast = _isFrontView;

        _backAnimator.gameObject.SetActive(!_isFrontView);
        _frontAnimator.gameObject.SetActive(_isFrontView);

        UpdateAnim();
        if (_isFrontView)
            _frontAnimator.AnimationState.GetCurrent(0).TrackTime =
                _backAnimator.AnimationState.GetCurrent(0).TrackTime;
        else
            _backAnimator.AnimationState.GetCurrent(0).TrackTime =
                _frontAnimator.AnimationState.GetCurrent(0).TrackTime;
    }

    private void UpdateAnim()
    {
        if (_isFrontView)
        {
            AnimationReferenceAsset currentFrontAnim = _currentAnim switch
            {
                AnimType.IDLE => _frontIdleAnim,
                AnimType.ATTACK_1 => _frontAttackAnim,
                AnimType.ATTACK_2 => _frontAttack2Anim,
                _ => throw new ArgumentOutOfRangeException()
            };
            TrackEntry track = _frontAnimator.AnimationState.SetAnimation(0, currentFrontAnim, false);
        }
        else
        {
            AnimationReferenceAsset currentFrontAnim = _currentAnim switch
            {
                AnimType.IDLE => _backIdleAnim,
                AnimType.ATTACK_1 => _backAttackAnim,
                AnimType.ATTACK_2 => _backAttack2Anim,
                _ => throw new ArgumentOutOfRangeException()
            };
            TrackEntry track = _backAnimator.AnimationState.SetAnimation(0, currentFrontAnim, false);
        }
    }

    private IEnumerator AttackAnimCoroutine()
    {
        while (true)
        {
            // PlayIdle();
            // yield return new WaitForSeconds(2f);
            //
            // PlayFirstAttack();
            // yield return new WaitForSeconds(2f);
            //
            // PlayIdle();
            // yield return new WaitForSeconds(2f);
            //
            // PlayFirstAttack();
            // yield return new WaitForSeconds(2f);
            //
            // PlayIdle();
            // yield return new WaitForSeconds(2f);

            yield return PlaySecondAttack();
        }
    }

    private void PlayIdle()
    {
        _currentAnim = AnimType.IDLE;
        UpdateAnim();
        _attack2System.Stop(true);
    }

    private void PlayFirstAttack()
    {
        _currentAnim = AnimType.ATTACK_1;
        UpdateAnim();
        _attack2System.Stop(true);
    }

    private IEnumerator PlaySecondAttack()
    {
        _currentAnim = AnimType.ATTACK_2;
        UpdateAnim();
        _attack2System.Play(true);

        // прицеливание огня
        float timer = 2f;
        while (timer > 0)
        {
            Vector3 direction = _target.position - _attack2System.transform.position;
            _attack2System.transform.rotation = Quaternion.FromToRotation(_attack2System.transform.position, _target.position);
            _attack2System.startLifetime = Vector3.Distance(_attack2System.transform.position, _target.position) / _attack2System.startSpeed;

            yield return null;

            timer -= Time.deltaTime;
        }
        // конец

        _attack2System.Stop(true);
    }

    private void MakeShoot()
    {
        _curveProjectile.Shoot(_muzzle.position, _target);
    }

    private void FrontAnimator_Event(TrackEntry trackentry, Event e)
    {
        if (e.Data.Name == "start")
        {
            MakeShoot();
        }
    }

    private void BackAnimator_Event(TrackEntry trackentry, Event e)
    {
        if (e.Data.Name == "start")
        {
            MakeShoot();
        }
    }

    private enum AnimType
    {
        IDLE,
        ATTACK_1,
        ATTACK_2,
    }
}