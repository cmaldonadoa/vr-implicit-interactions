using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameStep
{
    public bool completed = false;
    public bool disabled = false;

    public virtual void Init()
    {
        completed = false;
    }

    public virtual void Update()
    {
    }

    public virtual GameStep NextStep()
    {
        disabled = true;
        return this;
    }
}
