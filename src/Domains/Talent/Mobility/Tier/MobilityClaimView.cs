namespace Acme.Workforce.Talent.Mobility.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility claim.</summary>
public sealed record MobilityClaimView(string Code, string NameText, string ApprovedText, string Locale, string MaximumText, string Quantity, string Category, string Attempts, string BatchSizeText, string HoursText, string StartsAt, string StatusLabel)
{
    public static MobilityClaimView ToMobilityClaimView(MobilityClaim item)
    {
        return new MobilityClaimView(
            item.Code,
            Text.Truncate(item.Name, 80),
            $"{item.Approved}",
            item.Locale,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityClaimView> ToMobilityClaimViews(List<MobilityClaim> items)
    {
        var views = new List<MobilityClaimView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityClaimView(item));
        }
        return views;
    }

    public static double SumMobilityClaimView(MobilityClaimView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedQuantity + (double)parsedAttempts;
    }
}
