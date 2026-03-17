using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Floater:MonoBehaviour
{
    public float frequency = 2f;
    public float amplitude = 0.5f;

    private void LateUpdate()
    {
        var wave = Mathf.Sin(Time.time * frequency)* amplitude;
        transform.position += transform.up * wave * Time.deltaTime;
    }
}

