using System.Collections;
using UnityEngine;

namespace LostAndFound.Match3
{
    public class Match3Piece : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;

        public int Type { get; private set; }
        public int Column { get; private set; }
        public int Row { get; private set; }

        public void Initialize(int type, int column, int row, Sprite sprite, Color color, float size)
        {
            Type = type;
            Column = column;
            Row = row;

            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = color;
            spriteRenderer.sortingOrder = 1;

            transform.localScale = Vector3.one * size;
        }

        public void SetCoordinates(int column, int row)
        {
            Column = column;
            Row = row;
        }

        public void SetSelected(bool selected)
        {
            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = selected ? 5 : 1;

            transform.localScale = Vector3.one * (selected ? 1.0f : 0.86f);
        }

        public IEnumerator MoveTo(Vector3 target, float duration)
        {
            Vector3 start = transform.position;

            if (duration <= 0f)
            {
                transform.position = target;
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = 1f - Mathf.Pow(1f - t, 3f);
                transform.position = Vector3.Lerp(start, target, t);
                yield return null;
            }

            transform.position = target;
        }
    }
}
