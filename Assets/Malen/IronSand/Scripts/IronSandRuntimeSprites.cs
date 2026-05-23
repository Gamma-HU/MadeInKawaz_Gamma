using UnityEngine;

public static class IronSandRuntimeSprites
{
    private static Sprite circleSprite;
    private static Sprite barMagnetSprite;

    public static Sprite GetCircleSprite()
    {
        if (circleSprite == null)
        {
            circleSprite = CreateCircleSprite();
        }

        return circleSprite;
    }

    public static Sprite GetBarMagnetSprite()
    {
        if (barMagnetSprite == null)
        {
            barMagnetSprite = CreateBarMagnetSprite();
        }

        return barMagnetSprite;
    }

    private static Sprite CreateCircleSprite()
    {
        const int size = 16;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.45f;
        Color clear = new Color(1f, 1f, 1f, 0f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                texture.SetPixel(x, y, distance <= radius ? Color.white : clear);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    private static Sprite CreateBarMagnetSprite()
    {
        const int width = 48;
        const int height = 16;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        Color leftColor = new Color(0.9f, 0.1f, 0.08f, 1f);
        Color rightColor = new Color(0.08f, 0.24f, 0.9f, 1f);
        Color borderColor = new Color(0.95f, 0.95f, 0.95f, 1f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool border = x == 0 || y == 0 || x == width - 1 || y == height - 1;
                texture.SetPixel(x, y, border ? borderColor : (x < width / 2 ? leftColor : rightColor));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), height);
    }
}
