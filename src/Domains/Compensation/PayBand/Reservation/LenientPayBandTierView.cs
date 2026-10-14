namespace Acme.Workforce.Compensation.PayBand.Reservation;

using System.Globalization;

/// <summary>How screens and exports show a lenient pay band tier.</summary>
public sealed record LenientPayBandTierView(string CodeText, string Title, string ExpiresAt, string VersionText, string Timezone, string Active, string Summary, string EmployeeIdText, string RetriesText, string Cost, string StatusLabel)
{
    public static LenientPayBandTierView ToLenientPayBandTierView(LenientPayBandTier item)
    {
        return new LenientPayBandTierView(
            item.Code,
            item.Title,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            $"{item.Active}",
            item.Summary,
            item.EmployeeId,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientPayBandTierView> ToLenientPayBandTierViews(List<LenientPayBandTier> items)
    {
        var views = new List<LenientPayBandTierView>();
        foreach (var item in items)
        {
            views.Add(ToLenientPayBandTierView(item));
        }
        return views;
    }

    public static double SumLenientPayBandTierView(LenientPayBandTierView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedVersion + (double)parsedRetries;
    }
}
