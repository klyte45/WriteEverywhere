using Kwytto.Utils;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WriteEverywhere.Layout;
using WriteEverywhere.Localization;

namespace WriteEverywhere.Utils
{


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

