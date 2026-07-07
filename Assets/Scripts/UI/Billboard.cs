using UnityEngine;

namespace Kiosk.UI
{
    /// <summary>
    /// Richtet World-Space-Texte sauber zur Hauptkamera aus.
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        Camera _cachedCamera;

        void LateUpdate()
        {
            if (_cachedCamera == null || !_cachedCamera.isActiveAndEnabled)
                _cachedCamera = Camera.main;
            if (_cachedCamera == null) return;

            var localScale = transform.localScale;
            transform.localScale = new Vector3(Mathf.Abs(localScale.x), Mathf.Abs(localScale.y), Mathf.Abs(localScale.z));

            Vector3 facing = transform.position - _cachedCamera.transform.position;
            if (facing.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.LookRotation(facing.normalized, Vector3.up);
        }
    }
}
