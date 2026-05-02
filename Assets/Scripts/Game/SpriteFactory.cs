using UnityEngine;

namespace TimeTravelBanana.Game
{
    public static class SpriteFactory
    {
        private static Sprite _whiteSquare;
        private static Sprite _whiteCircle;

        public static Sprite WhiteSquare
        {
            get
            {
                if (_whiteSquare == null) _whiteSquare = MakeSquare(8);
                return _whiteSquare;
            }
        }

        public static Sprite WhiteCircle
        {
            get
            {
                if (_whiteCircle == null) _whiteCircle = MakeCircle(64);
                return _whiteCircle;
            }
        }

        private static Sprite MakeSquare(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color[size * size];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Sprite MakeCircle(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var px = new Color[size * size];
            float r = size / 2f - 0.5f;
            Vector2 c = new Vector2(size / 2f, size / 2f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c);
                    px[y * size + x] = d <= r ? Color.white : Color.clear;
                }
            }
            tex.SetPixels(px);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
