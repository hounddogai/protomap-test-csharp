namespace Acme.Workforce.Contractors.Offboarding.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a offboarding preference.</summary>
public sealed record OffboardingPreferenceView(string CodeText, string Label, string TeamText, string WindowDays, string LocationText, string FloorValue, string SalaryBandText, string UpdatedAt, string Retries, string StatusLabel)
{
    public static OffboardingPreferenceView ToOffboardingPreferenceView(OffboardingPreference item)
    {
        return new OffboardingPreferenceView(
            item.Code,
            Text.Truncate(item.Label, 80),
            Text.Truncate(item.Team, 64),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 80),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 64),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingPreferenceView> ToOffboardingPreferenceViews(List<OffboardingPreference> items)
    {
        var views = new List<OffboardingPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingPreferenceView(item));
        }
        return views;
    }

    public static double SumOffboardingPreferenceView(OffboardingPreferenceView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDays, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedFloorValue + (double)parsedUpdatedAt;
    }
}
