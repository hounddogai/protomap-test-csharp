namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled pay band manifest.</summary>
public sealed record ScheduledPayBandManifestView(string CodeText, string TitleText, string ExpiresAtText, string StartsAt, string CountryText, string Checksum, string ArchivedText, string WindowDays, string StatusLabel)
{
    public static ScheduledPayBandManifestView ToScheduledPayBandManifestView(ScheduledPayBandManifest item)
    {
        return new ScheduledPayBandManifestView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Country,
            item.Checksum,
            $"{item.Archived}",
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledPayBandManifestView> ToScheduledPayBandManifestViews(List<ScheduledPayBandManifest> items)
    {
        var views = new List<ScheduledPayBandManifestView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledPayBandManifestView(item));
        }
        return views;
    }

    public static double SumScheduledPayBandManifestView(ScheduledPayBandManifestView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedStartsAt + (double)parsedWindowDays;
    }
}
