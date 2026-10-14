namespace Acme.Workforce.Contractors.Offboarding.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal offboarding tier.</summary>
public sealed record SeasonalOffboardingTierView(string Code, string Label, string ExternalRefText, string SalaryBand, string WindowDays, string Quantity, string ThresholdText, string Reviewer, string Ratio, string Region, string StatusLabel)
{
    public static SeasonalOffboardingTierView ToSeasonalOffboardingTierView(SeasonalOffboardingTier item)
    {
        return new SeasonalOffboardingTierView(
            item.Code,
            item.Label,
            item.ExternalRef,
            item.SalaryBand,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalOffboardingTierView> ToSeasonalOffboardingTierViews(List<SeasonalOffboardingTier> items)
    {
        var views = new List<SeasonalOffboardingTierView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalOffboardingTierView(item));
        }
        return views;
    }

    public static double SumSeasonalOffboardingTierView(SeasonalOffboardingTierView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + (double)parsedQuantity + parsedThreshold;
    }
}
