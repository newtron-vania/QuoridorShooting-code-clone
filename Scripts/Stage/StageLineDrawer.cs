using UnityEngine;

public class StageLineDrawer
{
    private Sprite _lineSprite;
    private float _thickness;
    private Transform _parent;

    // Line Colors
    private readonly Color _activeColor = new Color(255f / 255f, 191f / 255f, 10f / 255f, 0.8f);
    private readonly Color _inactiveColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    public StageLineDrawer(float thickness, Transform parent)
    {
        _thickness = thickness;
        _parent = parent;
        CreateLineSprite();
    }

    private void CreateLineSprite()
    {
        if (_lineSprite != null) return;
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        _lineSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 2f);
    }

    public void DrawSegment(Vector3 startPos, Vector3 endPos, bool isHighlight)
    {
        GameObject lineObj = new GameObject("Line");
        if (_parent != null) lineObj.transform.SetParent(_parent);

        SpriteRenderer sr = lineObj.AddComponent<SpriteRenderer>();
        sr.sprite = _lineSprite;
        sr.sortingOrder = -1;

        sr.color = isHighlight ? _activeColor : _inactiveColor;

        Vector3 midPoint = (startPos + endPos) / 2f;
        lineObj.transform.position = midPoint;

        Vector3 direction = endPos - startPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        lineObj.transform.rotation = Quaternion.Euler(0, 0, angle);
        lineObj.transform.localScale = new Vector3(distance, _thickness, 1);
    }
}
