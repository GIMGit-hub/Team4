using UnityEngine;
using UnityEngine.InputSystem;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "StageSelectScene";

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            SceneFader.LoadScene(nextSceneName);
        }
    }
}