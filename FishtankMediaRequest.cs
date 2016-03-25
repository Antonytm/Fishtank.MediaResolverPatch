using System.Web;
using Sitecore.Resources.Media;
using System;
using Sitecore.Data.Items;
using Sitecore;
using Sitecore.Data;
using System.Globalization;
using System.Linq;


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



    public class FishtankMediaRequest : MediaRequest
    {
        protected override string GetMediaPath(string localPath)
        {
            int num = -1;
            string text = string.Empty;
            //No needs to decode path
            //string text2 = MainUtil.DecodeName(localPath);
            string text2 = localPath;
            foreach (string current in MediaManager.Provider.Config.MediaPrefixes.Select(new Func<string, string>(MainUtil.DecodeName)))
            {
                num = text2.IndexOf(current, System.StringComparison.InvariantCultureIgnoreCase);
                if (num >= 0)
                {
                    text = current;
                    break;
                }
            }
            if (num < 0)
            {
                return string.Empty;
            }
            if (string.Compare(text2, num, text, 0, text.Length, true, System.Globalization.CultureInfo.InvariantCulture) != 0)
            {
                return string.Empty;
            }
            string text3 = StringUtil.Divide(StringUtil.Mid(text2, num + text.Length), '.', true)[0];
            if (text3.EndsWith("/", System.StringComparison.InvariantCulture))
            {
                return string.Empty;
            }
            if (ShortID.IsShortID(text3))
            {
                return ShortID.Decode(text3);
            }
            string text4 = "/sitecore/media library/" + text3.TrimStart(new char[]
            {
                '/'
            });
            Database database = this.GetDatabase();
            if (database.GetItem(text4) == null)
            {
                Item item = database.GetItem("/sitecore/media library");
                if (item != null)
                {
                    text3 = StringUtil.Divide(StringUtil.Mid(localPath, num + text.Length), '.', true)[0];
                    Item item2 = this.PathResolver.ResolveItem(text3, item);
                    if (item2 != null)
                    {
                        text4 = item2.Paths.Path;
                    }
                }
            }
            return text4;
        }
    }

}
