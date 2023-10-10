using UnityEngine;

namespace Zappar.Examples
{
    public class WorldTrackingSample : MonoBehaviour
    {
        private ZapparWorldTrackingTarget m_tracker;

        private void Start()
        {
            m_tracker = GameObject.FindObjectOfType<ZapparWorldTrackingTarget>();
        }

        private void OnGUI()
        {
            if (m_tracker == null) return;
            if (m_tracker.UserHasPlaced)
            {
                Color guic = GUI.contentColor;
                GUI.contentColor = Color.white;
                int fontSize = GUI.skin.button.fontSize;
                GUI.skin.button.fontSize = 64;
                if (GUI.Button(new Rect(Screen.width / 2 - 150, Screen.height - 200, 300, 150),"Reset"))
                    m_tracker.ResetTrackerAnchor();
                GUI.contentColor = guic;
                GUI.skin.button.fontSize = fontSize;
            }
        }
    }
}