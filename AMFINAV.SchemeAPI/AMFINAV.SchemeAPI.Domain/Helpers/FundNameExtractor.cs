namespace AMFINAV.SchemeAPI.Domain.Helpers
{
    public static class FundNameExtractor
    {
        /// <summary>
        /// Input  → "Aditya Birla Sun Life Banking Fund - DIRECT - IDCW"
        /// Output → "Aditya Birla Sun Life Banking Fund"
        /// </summary>
        public static string Extract(string schemeName)
        {
            if (string.IsNullOrWhiteSpace(schemeName))
                return string.Empty;

            var index = schemeName.IndexOf(" - ", StringComparison.Ordinal);

            return index > 0
                ? schemeName.Substring(0, index).Trim()
                : schemeName.Trim();
        }
    }
}