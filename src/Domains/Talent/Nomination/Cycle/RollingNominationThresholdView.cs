namespace Acme.Workforce.Talent.Nomination.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling nomination threshold.</summary>
public sealed record RollingNominationThresholdView(string Code, string Title, string Category, string Locale, string Grade, string CostCenter, string Summary, string Weight, string Threshold, string Country, string StatusLabel)
{
    public static RollingNominationThresholdView ToRollingNominationThresholdView(RollingNominationThreshold item)
    {
        return new RollingNominationThresholdView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 64),
            item.Category,
            item.Locale,
            Text.Truncate(item.Grade, 80),
            item.CostCenter,
            Text.Truncate(item.Summary, 40),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingNominationThresholdView> ToRollingNominationThresholdViews(List<RollingNominationThreshold> items)
    {
        var views = new List<RollingNominationThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToRollingNominationThresholdView(item));
        }
        return views;
    }

    public static double SumRollingNominationThresholdView(RollingNominationThresholdView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedWeight + parsedThreshold;
    }
}
