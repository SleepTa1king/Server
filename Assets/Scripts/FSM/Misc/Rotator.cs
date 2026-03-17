using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class Rotator : MonoBehaviour
{
    protected Transform m_transform;
    public float rotateSpeed = 180f;
    protected Vector3 direction;
    public Space space;

    protected void Awake()
    {
        direction = new Vector3(0, -rotateSpeed, 0);
    }
    protected void LateUpdate()
    {
        transform.Rotate(direction * Time.deltaTime,space);
    }
}

