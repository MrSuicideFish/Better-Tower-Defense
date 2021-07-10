using System.Collections;
using UnityEngine;

public class Enemy_Spider : Unit
{
    protected override void MoveTo(Vector3 position)
    {
        GetAnimator().SetBool("Move Forward Fast", true);
        GetAnimator().SetBool("Move Forward Slow", false);
        base.MoveTo(position);
    }

    protected override void Attack(IBoardUnit boardUnit)
    {
        Stop();
        GetAnimator().SetBool("Move Forward Fast", false);
        GetAnimator().SetBool("Move Forward Slow", false);
        base.Attack(boardUnit);
    }
}