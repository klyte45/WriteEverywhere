using ColossalFramework;
using ColossalFramework.Globalization;
using Kwytto.LiteUI;
using Kwytto.UI;
using Kwytto.Utils;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Xml;

namespace WriteEverywhere.UI
{
    public class WTSOnNetParamsTab : WTSBaseParamsTab<OnNetInstanceCacheContainerXml>
    {
        private Vector2 m_tabViewScroll;

        public override Texture TabIcon { get; } = KResourceLoader.LoadTextureKwytto(CommonsSpriteNames.K45_FontIcon);

        protected override string GetAssetName(OnNetInstanceCacheContainerXml item) => item.Descriptor?.PropName;
        protected override void SetTextParameter(OnNetInstanceCacheContainerXml item, int currentEditingParam, string paramValue) => item.SetTextParameter(currentEditingParam, paramValue);
        protected override void DrawListing(Vector2 areaRect, OnNetInstanceCacheContainerXml item)
        {
            using (var scroll = new GUILayout.ScrollViewScope(m_tabViewScroll))
            {
                var paramsUsed = item.GetAllParametersUsedWithData();
                if ((paramsUsed?.Count ?? 0) > 0)
                {
                    foreach (var kv in paramsUsed.OrderBy(x => x.Key))
                    {
                        var contentTypes = kv.Value.GroupBy(x => x.textContent).Select(x => x.Key);
                        if (contentTypes.Count() > 1)
                        {
                            GUILayout.Label(string.Format(Locale.Get($"K45_WTS_ONNETEDITOR_TEXTPARAM"), kv.Key));
                            GUILayout.Label(Locale.Get($"K45_WTS_ONNETEDITOR_INVALIDPARAMSETTINGS_DIFFERENTKINDSAMEPARAM"), new GUIStyle(GUI.skin.label)
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
                        }
                        var usedByText = string.Join("\n", kv.Value.Select(x => $"\u2022{(x.ParameterDisplayName.IsNullOrWhiteSpace() ? x.SaveName : x.ParameterDisplayName)} ({(x.DefaultParameterValueAsString is null ? GUIKwyttoCommons.v_empty : $"<color=#{target}>{x.DefaultParameterValueAsString}</color>")})").ToArray());
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label(string.Format(Locale.Get($"K45_WTS_ONNETEDITOR_TEXTPARAM"), kv.Key) + $"\n<color=#{target}>{Locale.Get("K45_WTS_BOARD_TEXT_TYPE_DESC", targetContentType.ToString())}</color>\n\n{usedByText}");
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
