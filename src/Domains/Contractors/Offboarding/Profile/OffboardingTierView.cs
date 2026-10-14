namespace Acme.Workforce.Contractors.Offboarding.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a offboarding tier.</summary>
public sealed record OffboardingTierView(string Code, string Label, string WindowDays, string Skill, string Threshold, string TimezoneText, string MinimumText, string Margin, string Amount, string StatusLabel)
{
    public static OffboardingTierView ToOffboardingTierView(OffboardingTier item)
    {
        return new OffboardingTierView(
            item.Code,
            item.Label,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 40),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingTierView> ToOffboardingTierViews(List<OffboardingTier> items)
    {
        var views = new List<OffboardingTierView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingTierView(item));
        }
        return views;
    }

    public static double SumOffboardingTierView(OffboardingTierView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.MinimumText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedThreshold + parsedMinimum;
    }
}
