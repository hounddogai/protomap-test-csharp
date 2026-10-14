namespace Acme.Workforce.Compensation.MeritCycle.CycleRollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a merit cycle policy.</summary>
public sealed record MeritCyclePolicyView(string Code, string LabelText, string DurationMinutesText, string RetriesText, string Approved, string CostCenter, string ExpiresAt, string Location, string StatusLabel)
{
    public static MeritCyclePolicyView ToMeritCyclePolicyView(MeritCyclePolicy item)
    {
        return new MeritCyclePolicyView(
            item.Code,
            item.Label,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.CostCenter,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MeritCyclePolicyView> ToMeritCyclePolicyViews(List<MeritCyclePolicy> items)
    {
        var views = new List<MeritCyclePolicyView>();
        foreach (var item in items)
        {
            views.Add(ToMeritCyclePolicyView(item));
        }
        return views;
    }

    public static double SumMeritCyclePolicyView(MeritCyclePolicyView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedRetries + (double)parsedExpiresAt;
    }
}
