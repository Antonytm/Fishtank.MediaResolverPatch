using Sitecore.Resources.Media;
using System;
using Sitecore.Data.Items;
using Sitecore.Resources;
using Sitecore.Configuration;
using Sitecore;
using Sitecore.IO;
using Sitecore.Web;
using Sitecore.Data;
using System.Globalization;
using System.Xml;


namespace Fishtank.MediaResolverPatch
{
    /// <summary>
    /// Intercept the media request handler and apply custom caching to certain media files
    /// based on their extension and configured in GlobalConfiguration.MediaExtensionsNotToClientCache
    /// 
    /// This code base is taken from Sitecore.Kernel.DLL and altered so that media library requests aren't encoded. 
    /// Sitecore Patch Note:
    /// When rendering media URLs, the system did not use the configuration in the encodeNameReplacements section to replace special characters in the URLs. 
    /// This has been fixed so that media URLs also use the encodeNameReplacements configuration. (323105, 314977)
    /// </summary>
    

    using Assert = Sitecore.Diagnostics.Assert;
   
    public class FishtankMediaProvider : MediaProvider
    {
        private FishtankMediaConfig config;

        public override MediaConfig Config
        {
            get
            {
                return (MediaConfig)this.config;
            }
            set
            {
                Assert.ArgumentNotNull((object)value, "value");
                this.config = (FishtankMediaConfig)value;
            }
        }
        public FishtankMediaProvider()
        {
            XmlNode configNode = Factory.GetConfigNode("mediaLibrary");
            if (configNode != null)
                this.config = new FishtankMediaConfig(configNode);
            else
                this.config = new FishtankMediaConfig();
        }

        public override string GetMediaUrl(MediaItem item, MediaUrlOptions options)
        {
            Assert.ArgumentNotNull(item, "item");
            Assert.ArgumentNotNull(options, "options");
            bool flag = options.Thumbnail || this.HasMediaContent(item);
            if (!flag && item.InnerItem["path"].Length > 0)
            {
                if (!options.LowercaseUrls)
                {
                    return item.InnerItem["path"];
                }
                return item.InnerItem["path"].ToLowerInvariant();
            }
            else if (options.UseDefaultIcon && !flag)
            {
                if (!options.LowercaseUrls)
                {
                    return Themes.MapTheme(Settings.DefaultIcon);
                }
                return Themes.MapTheme(Settings.DefaultIcon).ToLowerInvariant();
            }
            else
            {
                Assert.IsTrue(this.Config.MediaPrefixes[0].Length > 0, "media prefixes are not configured properly.");
                string text = this.MediaLinkPrefix;
                if (options.AbsolutePath)
                {
                    text = options.VirtualFolder + text;
                }
                else if (text.StartsWith("/", System.StringComparison.InvariantCulture))
                {
                    text = StringUtil.Mid(text, 1);
                }
                //No needs to encode path
                //text = MainUtil.EncodePath(text, '/');
                if (options.AlwaysIncludeServerUrl)
                {
                    text = FileUtil.MakePath(string.IsNullOrEmpty(options.MediaLinkServerUrl) ? WebUtil.GetServerUrl() : options.MediaLinkServerUrl, text, '/');
                }
                string text2 = StringUtil.GetString(new string[]
                {
                    options.RequestExtension,
                    item.Extension,
                    "ashx"
                });
                text2 = StringUtil.EnsurePrefix('.', text2);
                string text3 = options.ToString();
                if (text3.Length > 0)
                {
                    text2 = text2 + "?" + text3;
                }
                string text4 = "/sitecore/media library/";
                string path = item.InnerItem.Paths.Path;
                string text5;
                if (options.UseItemPath && path.StartsWith(text4, System.StringComparison.OrdinalIgnoreCase))
                {
                    text5 = StringUtil.Mid(path, text4.Length);
                }
                else
                {
                    text5 = item.ID.ToShortID().ToString();
                }
                //No needs to encode path
                //text5 = MainUtil.EncodePath(text5, '/');
                text5 = text + text5 + (options.IncludeExtension ? text2 : string.Empty);
                if (!options.LowercaseUrls)
                {
                    return text5;
                }
                return text5.ToLowerInvariant();
            }
        }

    }
  
}
