using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WriteEverywhere.Font.Utility;

namespace WriteEverywhere.Font
{
    public class DynamicSpriteFont
    {
        internal struct TextureEnumerator : IEnumerable<Texture2D>
        {
            private readonly FontSystem _font;

            public TextureEnumerator(FontSystem font) => _font = font;

            public IEnumerator<Texture2D> GetEnumerator()
            {
                foreach (FontAtlas atlas in _font.Atlases)
                {
                    yield return atlas.Texture;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private readonly FontSystem _fontSystem;

        public long LastUpdate => _fontSystem.LastUpdateAtlas;

        public IEnumerable<Texture2D> Textures => new TextureEnumerator(_fontSystem);

        public int Height
        {
            get => _fontSystem.FontHeight;
            set => _fontSystem.FontHeight = value;
        }

        public float Spacing
        {
            get => _fontSystem.Spacing;
            set => _fontSystem.Spacing = value;
        }

        public bool UseKernings
        {
            get => _fontSystem.UseKernings;

            set => _fontSystem.UseKernings = value;
        }

        public int? DefaultCharacter
        {
            get => _fontSystem.DefaultCharacter;

            set => _fontSystem.DefaultCharacter = value;
        }

        public event Action CurrentAtlasFull
        {
            add
            {
                _fontSystem.CurrentAtlasFull += value;
            }

            remove
            {
                _fontSystem.CurrentAtlasFull -= value;
            }
        }

        private DynamicSpriteFont(byte[] ttf, string name, int defaultTargetHeight, int textureWidth, int textureHeight, float qualityMultiplier, Func<Shader> defaultShaderGetter, int blur)
        {
            _fontSystem = new FontSystem(textureWidth, textureHeight, defaultShaderGetter, blur)
            {
                FontHeight = defaultTargetHeight
            };

            _fontSystem.AddFontMem(ttf, qualityMultiplier);

        }

        public BasicRenderInformation DrawString(MonoBehaviour referenceGO, string text, Vector2 pos) => DrawString(referenceGO, text, pos, Vector2.one);

        public BasicRenderInformation DrawString(MonoBehaviour referenceGO, string text, Vector2 pos, Vector2 scale)
        {
            BasicRenderInformation result = _fontSystem.DrawText(referenceGO, pos.x, pos.y, text, scale);
            return result;
        }

        public void AddTtf(byte[] ttf, float qualityMultiplier) => _fontSystem.AddFontMem(ttf, qualityMultiplier);

        public void AddTtf(Stream ttfStream, float qualityMultiplier) => AddTtf(ttfStream.ToByteArray(), qualityMultiplier);

        public Vector2 MeasureString(string text)
        {
            var bounds = new WriteEverywhere.Font.Bounds();
            _fontSystem.TextBounds(0, 0, text, 1, ref bounds);

            return new Vector2(bounds.maxX, bounds.maxY);
        }

        public Texture2D DrawTextToTexture(string str, float spacing) => _fontSystem.WriteTexture2D(str, spacing);

        public Rect GetTextBounds(Vector2 position, string text, float scale)
        {
            var bounds = new WriteEverywhere.Font.Bounds();
            _fontSystem.TextBounds(position.x, position.y, text, 1, ref bounds);

            return new Rect((int)bounds.minX, (int)bounds.minY, (int)(bounds.maxX - bounds.minX) * scale, (int)(bounds.maxY - bounds.minY) * scale);
        }

        public void Reset(int width, int height) => _fontSystem.Reset(width, height);

        public void Reset() => _fontSystem.Reset();


        public static DynamicSpriteFont FromTtf(byte[] ttf, string name, int defaultTargetHeight, int textureWidth, int textureHeight, float qualityMultiplier, Func<Shader> defaultShaderGetter, int blur = 0) => new DynamicSpriteFont(ttf, name, defaultTargetHeight, textureWidth, textureHeight, qualityMultiplier, defaultShaderGetter, blur);


        public long GetCacheSize() => _fontSystem.GetCacheSize();
    }
}
