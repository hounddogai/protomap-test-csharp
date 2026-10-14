namespace Acme.Workforce.Contractors.Offboarding.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped offboarding baseline.</summary>
public sealed record CappedOffboardingBaselineView(string Code, string Title, string WindowDays, string Segment, string RatingText, string Amount, string BatchSize, string ChecksumText, string StatusLabel)
{
    public static CappedOffboardingBaselineView ToCappedOffboardingBaselineView(CappedOffboardingBaseline item)
    {
        return new CappedOffboardingBaselineView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Segment,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedOffboardingBaselineView> ToCappedOffboardingBaselineViews(List<CappedOffboardingBaseline> items)
    {
        var views = new List<CappedOffboardingBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToCappedOffboardingBaselineView(item));
        }
        return views;
    }

    public static double SumCappedOffboardingBaselineView(CappedOffboardingBaselineView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedRating + parsedAmount;
    }
}
