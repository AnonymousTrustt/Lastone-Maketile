using UnityEngine;

public static class SpriteSortingUtility
{
    public static void ApplySortingOrder(GameObject target, int sortingOrder)
    {
        if (target == null)
        {
            return;
        }

        foreach (SpriteRenderer spriteRenderer in target.GetComponentsInChildren<SpriteRenderer>())
        {
            spriteRenderer.sortingOrder = sortingOrder;
        }
    }

    public static void ApplyColor(GameObject target, Color color)
    {
        if (target == null)
        {
            return;
        }

        foreach (SpriteRenderer spriteRenderer in target.GetComponentsInChildren<SpriteRenderer>())
        {
            spriteRenderer.color = color;
        }
    }
}
