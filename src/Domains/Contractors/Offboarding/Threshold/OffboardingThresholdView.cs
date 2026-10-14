namespace Acme.Workforce.Contractors.Offboarding.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a offboarding threshold.</summary>
public sealed record OffboardingThresholdView(string CodeText, string TitleText, string WeightText, string Active, string Skill, string Hours, string DurationMinutesText, string Quantity, string StatusLabel)
{
    public static OffboardingThresholdView ToOffboardingThresholdView(OffboardingThreshold item)
    {
        return new OffboardingThresholdView(
            item.Code,
            Text.Truncate(item.Title, 80),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Skill,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingThresholdView> ToOffboardingThresholdViews(List<OffboardingThreshold> items)
    {
        var views = new List<OffboardingThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingThresholdView(item));
        }
        return views;
    }

    public static double SumOffboardingThresholdView(OffboardingThresholdView view)
    {
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return parsedWeight + parsedHours + (double)parsedDurationMinutes;
    }
}
