using UnityEngine;
using UnityEngine.InputSystem;

public class SceneChanger : MonoBehaviour
{
    [Header("“_–Åˆ—")]
    [SerializeField] private StartImageEffect startImageEffect;

    void Update()
    {
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (startImageEffect != null)
            {
                // æ‚ÉImage‚ğ“_–Å‚³‚¹‚é
                startImageEffect.OnStartClicked();
            }
            else
            {
                Debug.LogError(
                    "StartImageEffect ‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ"
                );
            }
        }
    }
}