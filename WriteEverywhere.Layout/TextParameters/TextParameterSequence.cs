using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WriteEverywhere.Xml
{
    public class TextParameterSequence : IEnumerable<TextParameterSequenceItem>
    {
        private TextParameterSequenceItem[] m_slides;
        private long m_totalLength;
        private readonly TextRenderingClass m_renderingClass;

        public int TotalItems => m_slides.Length;
        public TextParameterSequence(TextParameterSequenceItem[] slides, TextRenderingClass renderingClass)
        {
            m_slides = slides;
            m_totalLength = slides.Select(x => x.m_length).Sum();
            m_renderingClass = renderingClass;
        }

        public TextParameterWrapper GetAt(long ticks, long seed)
        {
            if (m_totalLength == 0)
            {
                return null;
            }

            var targetFrame = ((ticks + (seed * 1735847L)) & 0x7FFFFFFL) % m_totalLength;
            var ct = 0L;
            return m_slides[m_slides.TakeWhile(x => (ct += x.m_length) <= targetFrame).Count()].Value;
        }

        public void Add(TextParameterWrapper param, long time)
        {
            m_slides = m_slides.Concat(new[] { new TextParameterSequenceItem(param.ToString(), m_renderingClass, time) }).ToArray();
            m_totalLength = m_slides.Select(x => x.m_length).Sum();
        }

        public void MoveUp(int pos)
        {
            if (pos < 1 || pos >= m_slides.Length)
            {
                return;
            }
            m_slides = m_slides.Take(pos - 1).Concat(new[] { m_slides[pos], m_slides[pos - 1] }).Concat(m_slides.Skip(pos + 1)).ToArray();
        }
        public void MoveDown(int pos)
        {
            if (pos < 0 || pos >= m_slides.Length - 1)
            {
                return;
            }
            m_slides = m_slides.Take(pos).Concat(new[] { m_slides[pos + 1], m_slides[pos] }).Concat(m_slides.Skip(pos + 2)).ToArray();
        }

        public void SetLengthAt(int pos, long length)
        {
            if (pos < 0 || pos >= m_slides.Length)
            {
                return;
            }
            m_slides[pos].m_length = length;
            m_totalLength = m_slides.Select(x => x.m_length).Sum();
        }

        public void RemoveAt(int pos)
        {
            m_slides = m_slides.Where((x, i) => i != pos).ToArray();
            m_totalLength = m_slides.Select(x => x.m_length).Sum();
        }

        public TextParameterSequenceXml ToXml()
            => new TextParameterSequenceXml
            {
                Slides = m_slides.Select(x => new TextParameterSequenceSlideXml
                {
                    Value = x.Value.ToString(),
                    Frames = x.m_length
                }).ToArray(),
                RenderingClass = m_renderingClass,
            };
        public static TextParameterSequence FromXml(TextParameterSequenceXml input)
            => input is null
                ? null
                : new TextParameterSequence(input.Slides.Select(x => new TextParameterSequenceItem(x.Value, input.RenderingClass, x.Frames)).ToArray(), input.RenderingClass);
        public IEnumerator<TextParameterSequenceItem> GetEnumerator() => m_slides.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public void SetTextAt(int pos, string paramValue, TextRenderingClass vehicle)
        {
            var old = m_slides[pos];
            m_slides[pos] = new TextParameterSequenceItem(paramValue, vehicle, old.m_length);
        }
    }
}
