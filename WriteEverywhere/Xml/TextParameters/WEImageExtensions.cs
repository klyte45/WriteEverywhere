extern alias TLM;
using Kwytto.Utils;
using System.IO;

namespace WriteEverywhere.Xml
{
    internal static class WEImageExtensions
    {
        internal static void Save(this WEImageInfo info)
        {
            if (info.xmlPath != null)
            {
                var bordersOffsets = info.OffsetBorders;
                var xmlContent = new WEImageInfoXml
                {
                    borders = new WEImageInfoXml.BorderOffsets
                    {
                        bottom = bordersOffsets.bottom,
                        left = bordersOffsets.left,
                        right = bordersOffsets.right,
                        top = bordersOffsets.top,
                    },
                    pixelsPerMeters = info.PixelsPerMeter,
                };
                File.WriteAllText(info.xmlPath, XmlUtils.DefaultXmlSerialize(xmlContent, true));
                ModInstance.Controller.AtlasesLibrary.LoadImagesFromLocalFolders();
            }
        }
    }
}
