using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaypointManager:MonoBehaviour
{
    [Header("路径点设置")]
    public WaypointMode mode;
    public float waitTime;
    public List<Transform> waypoints;

    protected Transform m_current;
    protected bool m_pong;
    protected bool m_changing;

    public int index => waypoints.IndexOf(current);

    public virtual void Next()
    {
        if(m_changing)
        {
            return;
        }
        if(mode == WaypointMode.Pingpong)
        {
            if(!m_pong)
            {
                m_pong = (index + 1 == waypoints.Count);
            }
            else
            {
                m_pong = (index - 1 >= 0);
            }

            var next = !m_pong ? index + 1 : index - 1;
            StartCoroutine(Change(next));
        }
        else if(mode == WaypointMode.Loop)
        {
            if(index+1 < waypoints.Count)
            {
                StartCoroutine(Change(index + 1));
            }
            else
            {
                StartCoroutine(Change(0));
            }
        }
    }

    public virtual IEnumerator Change(int to)
    {
        m_changing = true;
        yield return new WaitForSeconds(waitTime);
        current = waypoints[to];
        m_changing = false;

    }
    public Transform current
    {
        get
        {
            if(!m_current)
            {
                m_current = waypoints[0];
            }
            return m_current;
        }
        protected set { m_current = value; }

    }
}

