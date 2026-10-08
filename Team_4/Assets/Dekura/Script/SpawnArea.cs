using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CardEffect;
using static UnityEngine.GraphicsBuffer;

public class SpawnArea : MonoBehaviour
{
    [SerializeField] private Color highlightColor;
    private Dictionary<Image, Color> defaultColor = new();

    [SerializeField] private List<Image> spawnAreaImage;

    public static EnemyUnit onCursolEnemy { get; private set; }

    private void OnEnable() => SetEventSubscribed(true);
    private void OnDisable() => SetEventSubscribed(false);

    private void SetEventSubscribed(bool isEnable)
    {
        CardController.OnDragStarted -= FindArea;
        if (isEnable) CardController.OnDragStarted += FindArea;
        CardController.OnDragEnded -= ResetHighlight;
        if (isEnable) CardController.OnDragEnded += ResetHighlight;
    }

    //---------------------------------------------------------------------------//

    private void FindArea(Vector2 pointerPosition, EffectTarget target)
    {
        if (target == EffectTarget.Player) return;

        bool isAnyHit = false;

        foreach (var image in spawnAreaImage)
        {
            if (FindEnemyImage(image) == null) continue;
            if (FindEnemyImage(image).GetComponentInChildren<EnemyUnit>().isDead) continue;

            RectTransform rect = image.rectTransform;
            bool isHit = RectTransformUtility.RectangleContainsScreenPoint(rect, pointerPosition);

            if (target == EffectTarget.AllEnemy)
                 isHit = RectTransformUtility.RectangleContainsScreenPoint(CardManager.Instance.hitColision, pointerPosition);

            if (isHit) 
            {
                SetHighlight(image, true);
                isAnyHit = true;
                continue;
            }
            SetHighlight(image, false);
        }

        if(!isAnyHit) ResetHighlight();
    }

    private void SetHighlight(Image spawnArea, bool enable)
    {
        if (!defaultColor.ContainsKey(spawnArea)) defaultColor[spawnArea] = FindEnemyImage(spawnArea).color;
        FindEnemyImage(spawnArea).color = enable
            ? highlightColor
            : defaultColor[spawnArea];

        if (enable) onCursolEnemy = FindEnemyImage(spawnArea).GetComponentInChildren<EnemyUnit>();
    }

    private void ResetHighlight()
    {
        foreach (var image in spawnAreaImage)
        {
            if (FindEnemyImage(image) == null) continue;
            if (FindEnemyImage(image).GetComponentInChildren<EnemyUnit>().isDead) continue;
            SetHighlight(image, false);
        }
        onCursolEnemy = null;
    }

    private Image FindEnemyImage(Image spawnArea)
    {
        EnemyUnit enemy = spawnArea.GetComponentInChildren<EnemyUnit>();
        if (enemy != null)
        {
            Image enemyImage = enemy.GetComponentInChildren<Image>();
            return enemyImage;
        }
        return null;
    }
}
