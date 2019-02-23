namespace Crawler.Pixiv
{
    public enum RankMode
    {
        Daily,
        DailyR18,
        Female,
        FemaleR18,
        Male,
        MaleR18,
        Monthly,
        /// <summary>
        /// 原创
        /// </summary>
        Original,
        /// <summary>
        /// 新人
        /// </summary>
        Rookie,
        Weekly,
        WeeklyR18,
    }

    public static class RankModeExtension
    {
        public static bool IsR18(this RankMode mode)
        {
            switch (mode)
            {
                case RankMode.DailyR18: return true;
                case RankMode.FemaleR18: return true;
                case RankMode.MaleR18: return true;
                case RankMode.WeeklyR18: return true;
                default: return false;
            }
        }

        public static string Stringify(this RankMode mode)
        {
            switch (mode)
            {
                case RankMode.Daily: return "daily";
                case RankMode.DailyR18: return "daily_r18";
                case RankMode.Female: return "female";
                case RankMode.FemaleR18: return "female_r18";
                case RankMode.Male: return "male";
                case RankMode.MaleR18: return "male_r18";
                case RankMode.Monthly: return "monthly";
                case RankMode.Original: return "original";
                case RankMode.Rookie: return "rookie";
                case RankMode.Weekly: return "weekly";
                case RankMode.WeeklyR18: return "weekly_r18";
                //Daily
                default: return "daily";
            }
        }
    }
}