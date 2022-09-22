extern alias ADR;
using Kwytto.Utils;
using SpriteFontPlus;
using SpriteFontPlus.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WriteEverywhere.Data;
using WriteEverywhere.Singleton;
using WriteEverywhere.Utils;
using WriteEverywhere.Xml;
using static ADR::Bridge_WE2ADR.IBridge;

namespace WriteEverywhere.Sprites
{
    public class WTSHighwayShieldsAtlasLibrary : MonoBehaviour
    {

        public void Awake() => ResetHwShieldAtlas();

        #region Highway shields
        private readonly Dictionary<string, WEImageInfo> m_hwShieldsAtlas = new Dictionary<string, WEImageInfo>();
        private Dictionary<ushort, BasicRenderInformation> HighwayShieldsCache { get; } = new Dictionary<ushort, BasicRenderInformation>();

        private void ResetHwShieldAtlas()
        {
            m_hwShieldsAtlas.Clear();
        }
        public void PurgeShields()
        {
            HighwayShieldsCache.Clear();
            ResetHwShieldAtlas();
        }

        public BasicRenderInformation DrawHwShield(ushort seedId)
        {
            if (HighwayShieldsCache.TryGetValue(seedId, out BasicRenderInformation bri))
            {
                if (bri != null)
                {
                    return bri;
                }
            }
            else
            {
                HighwayShieldsCache[seedId] = null;
                if (ModInstance.Controller.ConnectorADR.AddressesAvailable)
                {
                    StartCoroutine(WriteHwShieldTextureCoroutine(seedId));
                }
            }

            return null;

        }
        private IEnumerator WriteHwShieldTextureCoroutine(ushort seedId)
        {

            string id = $"{seedId}";

            if (m_hwShieldsAtlas[id] is null)
            {
                yield return 0;
                while (!CheckHwShieldCoroutineCanContinue())
                {
                    yield return null;
                }
                var hwData = ModInstance.Controller.ConnectorADR.GetHighwayData(seedId);
                if (hwData is null)
                {
                    yield break;
                }
                WTSHighwayShieldsSingleton.GetTargetDescriptor(hwData.layoutName ?? "", out ConfigurationSource src, out HighwayShieldDescriptor layoutDescriptor);
                if (src == ConfigurationSource.NONE)
                {
                    yield break;
                }

                var drawingCoroutine = CoroutineWithData.From(this, RenderHighwayShield(
                    FontServer.instance[layoutDescriptor.FontName] ?? FontServer.instance[WTSEtcData.Instance.FontSettings.HighwayShieldsFont] ?? FontServer.instance[MainController.DEFAULT_FONT_KEY],
                    layoutDescriptor, hwData));
                yield return drawingCoroutine.Coroutine;
                while (!CheckHwShieldCoroutineCanContinue())
                {
                    yield return null;
                }
                HighwayShieldsCache.Clear();
                StopAllCoroutines();
                yield break;
            }
            yield return 0;
            var bri = new BasicRenderInformation
            {
                m_YAxisOverflows = new RangeVector { min = 0, max = 20 },
            };

            yield return 0;
            WTSAtlasesLibrary.BuildMeshFromAtlas(bri, m_hwShieldsAtlas[id]);
            WTSAtlasesLibrary.RegisterMeshSingle(seedId, bri, HighwayShieldsCache);
            yield break;
        }
        private static bool CheckHwShieldCoroutineCanContinue()
        {
            if (m_lastCoroutineStepHS != SimulationManager.instance.m_currentTickIndex)
            {
                m_lastCoroutineStepHS = SimulationManager.instance.m_currentTickIndex;
                m_coroutineCounterHS = 0;
            }
            if (m_coroutineCounterHS >= 1)
            {
                return false;
            }
            m_coroutineCounterHS++;
            return true;
        }

        private static IEnumerator<Texture2D> RenderHighwayShield(DynamicSpriteFont defaultFont, HighwayShieldDescriptor descriptor, AdrHighwayParameters parameters)
        {
            if (defaultFont is null)
            {
                defaultFont = FontServer.instance[WTSEtcData.Instance.FontSettings.GetTargetFont(FontClass.HighwayShields)];
            }

            WEImageInfo spriteInfo = descriptor.BackgroundImageParameter.GetCurrentWEImageInfo(null);
            if (spriteInfo is null)
            {
                LogUtils.DoWarnLog("HW: Background info is invalid for hw shield descriptor " + descriptor.SaveName);
                yield break;
            }
            else
            {

                int shieldHeight = WTSAtlasLoadingUtils.MAX_SIZE_IMAGE_IMPORT;
                int shieldWidth = WTSAtlasLoadingUtils.MAX_SIZE_IMAGE_IMPORT;
                var shieldTexture = new Texture2D(spriteInfo.Texture.width, spriteInfo.Texture.height);
                var targetColor = descriptor.BackgroundColorIsFromHighway && parameters.hwColor != default ? parameters.hwColor : descriptor.BackgroundColor;
                shieldTexture.SetPixels(spriteInfo.Texture.GetPixels().Select(x => x.MultiplyChannelsButAlpha(targetColor)).ToArray());
                TextureScaler.scale(shieldTexture, shieldWidth, shieldHeight);
                Color[] formTexturePixels = shieldTexture.GetPixels();

                foreach (var textDescriptor in descriptor.TextDescriptors)
                {
                    //if (!textDescriptor.GetTargetText(parameters, out string text))
                    //{
                    //    continue;
                    //}

                    //Texture2D overlayTexture;
                    //if (text is null && textDescriptor.m_textType == TextType.GameSprite)
                    //{
                    //    var spriteTexture = textDescriptor.m_paramValue?.GetCurrentWEImageInfo(null)?.texture;
                    //    if (spriteTexture is null)
                    //    {
                    //        continue;
                    //    }
                    //    overlayTexture = new Texture2D(spriteTexture.width, spriteTexture.height);
                    //    overlayTexture.SetPixels(spriteTexture.GetPixels());
                    //    overlayTexture.Apply();
                    //}
                    //else if (text is null)
                    //{
                    //    continue;
                    //}
                    //else
                    //{
                    //    var targetFont = FontServer.instance[textDescriptor.m_overrideFont] ?? FontServer.instance[WTSEtcData.Instance.FontSettings.GetTargetFont(textDescriptor.m_fontClass)] ?? defaultFont;
                    //    overlayTexture = targetFont.DrawTextToTexture(text, textDescriptor.m_charSpacingFactor);
                    //}

                    //if (overlayTexture is null)
                    //{
                    //    continue;
                    //}

                    //Color textColor;
                    //switch (textDescriptor.ColoringConfig.ColorSource)
                    //{
                    //    case ColoringSettings.ColoringSource.Contrast:
                    //        textColor = targetColor.ContrastColor();
                    //        break;
                    //    case ColoringSettings.ColoringSource.Parent:
                    //        textColor = targetColor;
                    //        break;
                    //    case ColoringSettings.ColoringSource.Fixed:
                    //    default:
                    //        textColor = textDescriptor.ColoringConfig.m_cachedColor;
                    //        break;
                    //}

                    //Color[] overlayColorArray = overlayTexture.GetPixels().Select(x => new Color(textColor.r, textColor.g, textColor.b, x.a)).ToArray();

                    //var textAreaSize = textDescriptor.GetAreaSize(shieldWidth, shieldHeight, overlayTexture.width, overlayTexture.height, true);
                    //TextureScaler.scale(overlayTexture, Mathf.FloorToInt(textAreaSize.z), Mathf.FloorToInt(textAreaSize.w));

                    //Color[] textColors = overlayTexture.GetPixels();
                    //int textWidth = overlayTexture.width;
                    //int textHeight = overlayTexture.height;
                    //Destroy(overlayTexture);


                    //Task<Tuple<Color[], int, int>> task = ThreadHelper.taskDistributor.Dispatch(() =>
                    //{
                    //    int topMerge = Mathf.RoundToInt(textAreaSize.y);
                    //    int leftMerge = Mathf.RoundToInt(textAreaSize.x);
                    //    try
                    //    {
                    //        TextureRenderUtils.MergeColorArrays(colorOr: formTexturePixels,
                    //                                            widthOr: shieldWidth,
                    //                                            colors: textColors.Select(x => x.MultiplyChannelsButAlpha(textColor)).ToArray(),
                    //                                            startX: leftMerge,
                    //                                            startY: topMerge,
                    //                                            sizeX: textWidth,
                    //                                            sizeY: textHeight);
                    //    }
                    //    catch (Exception e)
                    //    {
                    //        LogUtils.DoErrorLog($"Exception while writing text in the shield: {e.Message}\n{e.StackTrace}\n\nDescriptor:{JsonUtility.ToJson(descriptor)}\ntextDescriptor: {textDescriptor?.SaveName}");
                    //    }
                    //    return Tuple.New(formTexturePixels, shieldWidth, shieldHeight);
                    //});
                    //while (!task.hasEnded || m_coroutineCounterHS > 1)
                    //{
                    //    if (task.hasEnded)
                    //    {
                    //        m_coroutineCounterHS++;
                    //    }
                    //    yield return null;
                    //    if (m_lastCoroutineStepHS != SimulationManager.instance.m_currentTickIndex)
                    //    {
                    //        m_lastCoroutineStepHS = SimulationManager.instance.m_currentTickIndex;
                    //        m_coroutineCounterHS = 0;
                    //    }
                    //}
                    //m_coroutineCounterHS++;
                    //formTexturePixels = task.result.First;
                }
                shieldTexture.SetPixels(formTexturePixels);
                shieldTexture.Apply();
                yield return shieldTexture;
            }
        }


        private static uint m_lastCoroutineStepHS = 0;
        private static uint m_coroutineCounterHS = 0;
        #endregion
    }
}