using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
public class Unit : MonoBehaviour, IBoardUnit
{
    private int _health;
    private int _armor;
    private StatusEffect _status;

    private NavMeshAgent _agent;
    public NavMeshAgent GetNavAgent()
    {
        return _agent;
    }

    private Animator _animator;
    public Animator GetAnimator()
    {
        return _animator;
    }

    private UnitCfg _cfg;

    private bool _isAttacking;

    public void SetCfg(UnitCfg cfg)
    {
        _cfg = cfg;
    }

    public virtual void OnSpawned()
    {
        _agent = this.GetComponent<NavMeshAgent>();
        _animator = this.GetComponentInChildren<Animator>();

        _health = _cfg.health;
        _armor = _cfg.armor;
        _status = StatusEffect.None;
        
        // set agent properties
        _agent.speed = _cfg.moveSpeed;
        
        // set animator properties
        // ...

        // spawn a fire effect

        this.StartCoroutine(OnSpawnedRoutine());
    }

    public virtual void OnKilled()
    {
        _health = 0;
        _armor = 0;
    }

    public int GetHealth()
    {
        return _health;
    }

    public int GetArmor()
    {
        return _armor;
    }

    public StatusEffect GetStatus()
    {
        return _status;
    }

    public void OnHit(HitInfo hitInfo)
    {
        int armorDiff = 0;
        if (_armor > 0)
        {
            armorDiff = _armor -= hitInfo.damage;
        }

        if (armorDiff < 0)
        {
            _armor = 0;
            _health -= Mathf.Abs(armorDiff);
            if (_health <= 0)
            {
                UnitManager.Get().KillUnit(this);
                GetAnimator().SetTrigger("Die");
                GetAnimator().ResetTrigger("Die");
                return;
            }
        }
        
        GetAnimator().SetTrigger("Take Damage");
        GetAnimator().ResetTrigger("Take Damage");
    }

    public virtual void OnBoardUnitUpdate()
    {
        if (!_isAttacking)
        {
            if (!_cfg.attacksMultipleEnemies)
            {
                IBoardUnit target = GetTargetInRange();
                if (target != null)
                {
                    Attack(target);
                }
            }
            else
            {
                IBoardUnit[] targets = GetTargetsInRange();
                if (targets != null && targets.Length > 0)
                {
                    if (targets.Contains(GameManager.Instance.nexus))
                    {
                        Attack(GameManager.Instance.nexus);
                    }
                    else
                    {
                        Attack(targets[0]);
                    }
                }
            }
        }
        else if(GetNavAgent().isStopped)
        {
            MoveTo(GameManager.Instance.nexus.transform.position);
        }
    }

    protected virtual void MoveTo(Vector3 position)
    {
        if (GetNavAgent().SetDestination(position))
        {
        }
    }

    protected void Stop()
    {
        GetNavAgent().isStopped = true;
    }

    protected virtual void Attack(IBoardUnit boardUnit)
    {
    }

    protected IBoardUnit GetTargetInRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position,
            _cfg.attackRange,
            1 << LayerMask.NameToLayer("FriendlyUnit"));
        
        if (hitColliders == null || hitColliders.Length == 0) return null;
        for (int i = 0; i < hitColliders.Length; i++)
        {
            IBoardUnit target = hitColliders[i].GetComponent<IBoardUnit>();
            if (target != null) return target;
        }

        return null;
    }

    protected IBoardUnit[] GetTargetsInRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position,
            _cfg.attackRange,
            1 << LayerMask.NameToLayer("FriendlyUnit"));

        if (hitColliders != null && hitColliders.Length > 0)
        {
            List<IBoardUnit> targets = new List<IBoardUnit>();
            for (int i = 0; i < hitColliders.Length; i++)
            {
                IBoardUnit target = hitColliders[i].GetComponent<IBoardUnit>();
                if (target != null)
                {
                    targets.Add(target);
                }
            }

            return targets.ToArray();
        }
        return null;
    }

    private IEnumerator OnSpawnedRoutine()
    {
        // do spawned animation
        
        // wait x time
        
        // begin pathing
        var nexusPos = GameManager.Instance.nexus.transform.position;
        var rand = Random.insideUnitCircle * 2;
        nexusPos.x += rand.x;
        nexusPos.z += rand.y;
        
        MoveTo(nexusPos);

        yield return null;
    }
}