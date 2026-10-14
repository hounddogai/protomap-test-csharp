namespace Acme.Workforce.Contractors.Offboarding.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted offboarding threshold.</summary>
public sealed record WeightedOffboardingThresholdView(string Code, string Label, string ContactEmail, string UnitText, string Checksum, string LeaveDaysText, string Visible, string Version, string StatusLabel)
{
    public static WeightedOffboardingThresholdView ToWeightedOffboardingThresholdView(WeightedOffboardingThreshold item)
    {
        return new WeightedOffboardingThresholdView(
            item.Code,
            item.Label,
            Text.Truncate(item.ContactEmail, 120),
            Text.Truncate(item.Unit, 40),
            item.Checksum,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedOffboardingThresholdView> ToWeightedOffboardingThresholdViews(List<WeightedOffboardingThreshold> items)
    {
        var views = new List<WeightedOffboardingThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedOffboardingThresholdView(item));
        }
        return views;
    }

    public static double SumWeightedOffboardingThresholdView(WeightedOffboardingThresholdView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedVersion;
    }
}
