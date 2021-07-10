using UnityEngine;

public class Nexus : MonoBehaviour, IBoardUnit
{
    public int TotalHealth;
    public int TotalArmor;

    private int _health;
    private int _armor;
    
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
        return StatusEffect.None;
    }

    public void OnSpawned()
    {
        _health = TotalHealth;
        _armor = TotalArmor;
    }

    public void OnKilled()
    {
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
                GameManager.Instance.EndGame(false);
                return;
            }
        }
    }

    public void OnBoardUnitUpdate()
    {
        if (_health <= 0)
        {
            GameManager.Instance.EndGame(false);
        }
    }
}