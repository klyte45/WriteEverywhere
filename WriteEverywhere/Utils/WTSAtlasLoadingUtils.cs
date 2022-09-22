using Kwytto.Utils;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using WriteEverywhere.Localization;

namespace WriteEverywhere.Utils
{
    [XmlRoot("ImageInformation")]
    public class WEImageInfoXml
    {
        [XmlElement("Borders")]
        public BorderOffsets borders;

        [XmlAttribute("pixelsPerMeters")]
        public float pixelsPerMeters = 100;

        public class BorderOffsets
        {
            [XmlAttribute("pxLeft")]
            public int left;
            [XmlAttribute("pxRight")]
            public int right;
            [XmlAttribute("pxTop")]
            public int top;
            [XmlAttribute("pxBottom")]
            public int bottom;

            public Vector4 ToWEBorder(float width, float height) => new Vector4(left / width, right / width, top / height, bottom / height);
        }
    }

    public class WEImageInfo
    {
        public WEImageInfo(string xmlPath)
        {
            this.xmlPath = xmlPath;
        }
        public readonly string xmlPath;
        public Vector4 Borders { get; set; }
        public string Name { get; set; }
        public Texture2D Texture { get; set; }
        public float PixelsPerMeter { get; set; }
        public RectOffset OffsetBorders => new RectOffset(Mathf.RoundToInt(Borders.x * Texture.width), Mathf.RoundToInt(Borders.y * Texture.width), Mathf.RoundToInt(Borders.z * Texture.height), Mathf.RoundToInt(Borders.w * Texture.height));

        internal void Save()
        {
            if (xmlPath != null)
            {
                var bordersOffsets = OffsetBorders;
                var xmlContent = new WEImageInfoXml
                {
                    borders = new WEImageInfoXml.BorderOffsets
                    {
                        bottom = bordersOffsets.bottom,
                        left = bordersOffsets.left,
                        right = bordersOffsets.right,
                        top = bordersOffsets.top,
                    },
                    pixelsPerMeters = PixelsPerMeter,
                };
                File.WriteAllText(xmlPath, XmlUtils.DefaultXmlSerialize(xmlContent, true));
                ModInstance.Controller.AtlasesLibrary.LoadImagesFromLocalFolders();
            }
        }
    }

    public static class WTSAtlasLoadingUtils
    {
        public const int MAX_SIZE_IMAGE_IMPORT = 512;
        public static void LoadAllImagesFromFolder(string folder, out List<WEImageInfo> spritesToAdd, out List<string> errors, bool addPrefix = true)
        {
            spritesToAdd = new List<WEImageInfo>();
            errors = new List<string>();
            LoadAllImagesFromFolderRef(folder, ref spritesToAdd, ref errors, addPrefix);
        }
        public static void LoadAllImagesFromFolderRef(string folder, ref List<WEImageInfo> spritesToAdd, ref List<string> errors, bool addPrefix)
        {
            foreach (var imgFile in Directory.GetFiles(folder, "*.png"))
            {
                var fileData = File.ReadAllBytes(imgFile);
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                var xmlInfoFileName = imgFile.Replace(".png", "_info.xml");
                WEImageInfoXml xmlInfo = null;
                if (File.Exists(xmlInfoFileName))
                {
                    xmlInfo = XmlUtils.DefaultXmlDeserialize<WEImageInfoXml>(File.ReadAllText(xmlInfoFileName));
                }
                if (tex.LoadImage(fileData))
                {
                    if (tex.width <= MAX_SIZE_IMAGE_IMPORT && tex.width <= MAX_SIZE_IMAGE_IMPORT)
                    {
                        var imgName = addPrefix ? $"K45_WE_{Path.GetFileNameWithoutExtension(imgFile)}" : Path.GetFileNameWithoutExtension(imgFile);
                        spritesToAdd.Add(new WEImageInfo(xmlInfoFileName)
                        {
                            Borders = xmlInfo?.borders.ToWEBorder(tex.width, tex.height) ?? default,
                            Name = imgName,
                            Texture = tex,
                            PixelsPerMeter = xmlInfo?.pixelsPerMeters ?? 100
                        });
                    }
                    else
                    {
                        errors.Add($"{Path.GetFileName(imgFile)}: {Str.WTS_CUSTOMSPRITE_IMAGETOOLARGE} (max: 400x400)");
                    }
                }
                else
                {
                    errors.Add($"{Path.GetFileName(imgFile)}: {Str.WTS_CUSTOMSPRITE_FAILEDREADIMAGE}");
                }
            }
        }
    }
}

