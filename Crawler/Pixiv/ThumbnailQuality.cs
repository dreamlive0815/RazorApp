namespace Crawler.Pixiv
{
    public enum ThumbnailQuality
    {
        S128,
        S240,
        S360,
        S480,

        Mini,
        Original,
        Regular,
        Small,
        Thumbnail,
    }

    public static class ThumbnailQualityExtension
    {
        public static string Stringify(this ThumbnailQuality quality)
        {
            switch (quality)
            {
                case ThumbnailQuality.S128: return "128x128";
                case ThumbnailQuality.S240: return "240mw";
                case ThumbnailQuality.S360: return "360x360";
                case ThumbnailQuality.S480: return "480mw";

                case ThumbnailQuality.Mini: return "mini";
                case ThumbnailQuality.Original: return "original";
                case ThumbnailQuality.Regular: return "regular";
                case ThumbnailQuality.Small: return "small";
                case ThumbnailQuality.Thumbnail: return "thumb";
                //Regular
                default: return "regular";
            }
        }
    }
}