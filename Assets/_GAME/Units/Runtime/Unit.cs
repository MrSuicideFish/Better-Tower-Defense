using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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

    public virtual void OnSpawned()
    {
        _agent = this.GetComponent<NavMeshAgent>();
        _animator = this.GetComponentInChildren<Animator>();
        
        
        // spawn a fire effect

        this.StartCoroutine(OnSpawnedRoutine());
    }

    public virtual void OnKilled()
    {
        
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

    protected void MoveTo(Vector3 position)
    {
        if (_agent.SetDestination(position))
        {
            
        }
    }

    public virtual void Attack(IBoardUnit boardUnit)
    {
        
    }

    private IEnumerator OnSpawnedRoutine()
    {
        // do spawned animation
        
        // wait x time
        
        // begin pathing
        var nexusPos = GameManager.Instance.nexus.transform.position;
        MoveTo(nexusPos);

        yield return null;
    }
}