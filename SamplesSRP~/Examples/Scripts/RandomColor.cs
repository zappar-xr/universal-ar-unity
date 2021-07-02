using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zappar.Examples
{
    public class RandomColor : MonoBehaviour
    {
        private Material m_Mat;

        void OnEnable()
        {
            m_Mat = transform.GetComponent<Renderer>()?.material;
        }

        public void OnMouseDown()
        {
            if(m_Mat!=null)
            {
                m_Mat.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            }
        }
    }
}