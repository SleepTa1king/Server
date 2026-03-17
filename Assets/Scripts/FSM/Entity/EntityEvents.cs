using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class EntityEvents
{
    public UnityEvent OnGroundEnter;
    public UnityEvent OnGroundExit;
    public UnityEvent OnRailsEnter;
    public UnityEvent OnRailsExit;
}

