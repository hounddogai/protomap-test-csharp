namespace Acme.Workforce.Contractors.Rate.RecordItem;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rate manifest.</summary>
public sealed record RateManifestView(string Code, string Label, string Checksum, string ExpiresAtText, string DurationMinutesText, string LeaveDays, string BatchSize, string CategoryText, string EndsAt, string Amount, string StatusLabel)
{
    public static RateManifestView ToRateManifestView(RateManifest item)
    {
        return new RateManifestView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Label, 40),
            item.Checksum,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RateManifestView> ToRateManifestViews(List<RateManifest> items)
    {
        var views = new List<RateManifestView>();
        foreach (var item in items)
        {
            views.Add(ToRateManifestView(item));
        }
        return views;
    }

    public static double SumRateManifestView(RateManifestView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedDurationMinutes + (double)parsedLeaveDays;
    }
}
