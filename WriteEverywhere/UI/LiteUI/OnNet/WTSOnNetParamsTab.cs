using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Localization;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    public class WTSOnNetParamsTab : WTSBaseParamsTab<OnNetInstanceCacheContainerXml>
    {
        private Vector2 m_tabViewScroll;
        private uint lastTickDraw;
        private Dictionary<int, List<Tuple<IParameterizableVariable, string>>> m_cachedParamsUsed;

        public override Texture TabIcon { get; } = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_AbsoluteMode);

        protected override string GetAssetName(OnNetInstanceCacheContainerXml item) => item.m_simplePropName;
        protected override void SetTextParameter(OnNetInstanceCacheContainerXml item, int currentEditingParam, string paramValue) => item.SetTextParameter(currentEditingParam, paramValue);
        protected override void DrawListing(Vector2 areaRect, OnNetInstanceCacheContainerXml item)
        {
            using (var scroll = new GUILayout.ScrollViewScope(m_tabViewScroll))
            {
                if (SimulationManager.instance.m_currentTickIndex - lastTickDraw > 5)
                {
                    m_cachedParamsUsed = item.GetAllParametersUsedWithData();
                }
                lastTickDraw = SimulationManager.instance.m_currentTickIndex;
                if ((m_cachedParamsUsed?.Count ?? 0) > 0)
                {
                    foreach (var kv in m_cachedParamsUsed.OrderBy(x => x.Key))
                    {
                        var contentTypes = kv.Value.GroupBy(x => x.First.GetTextContent()).Select(x => x.Key);
                        if (contentTypes.Count() > 1)
                        {
                            GUILayout.Label(string.Format(Str.WTS_ONNETEDITOR_TEXTPARAM, kv.Key));
                            GUILayout.Label(Str.WTS_ONNETEDITOR_INVALIDPARAMSETTINGS_DIFFERENTKINDSAMEPARAM, new GUIStyle(GUI.skin.label)
                            {
                                alignment = TextAnchor.MiddleCenter
                            });
                            GUILayout.Space(4);
                            continue;
                        }
                        var targetContentType = contentTypes.First();
                        string target = "FFFFFF";
                        switch (targetContentType)
                        {
                            case TextContent.ParameterizedSpriteSingle:
                                target = Image;
                                break;
                            case TextContent.ParameterizedSpriteFolder:
                                target = Folder;
                                break;
                            case TextContent.ParameterizedText:
                                target = Text;
                                break;
                            case TextContent.Any:
                                target = Any;
                                break;
                        }
                        var usedByText = string.Join("\n", kv.Value.Select(x => $"\u2022{(x.First.GetParameterDisplayName() ?? x.Second)}").ToArray());
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label(string.Format(Str.WTS_ONNETEDITOR_TEXTPARAM, kv.Key) + $"\n<color=#{target}>{targetContentType.ValueToI18n()}</color>\n\n{usedByText}");
                            var param = item.GetParameter(kv.Key);
                            if (GUILayout.Button(param is null ? GUIKwyttoCommons.v_null : param.IsEmpty ? GUIKwyttoCommons.v_empty : param.ToString(), GUILayout.ExpandHeight(true)))
                            {
                                if (param is null)
                                {
                                    item.SetTextParameter(kv.Key, null);
                                    param = item.GetParameter(kv.Key);
                                }
                                GoToPicker(kv.Key, targetContentType, param, item);
                            }
                        };

                    }

                }
                m_tabViewScroll = scroll.scrollPosition;
            }
        }


    }
}
