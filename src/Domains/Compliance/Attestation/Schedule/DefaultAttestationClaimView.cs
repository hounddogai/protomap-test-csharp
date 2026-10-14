namespace Acme.Workforce.Compliance.Attestation.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default attestation claim.</summary>
public sealed record DefaultAttestationClaimView(string Code, string Name, string Summary, string WindowDaysText, string RateText, string Timezone, string CostCenter, string Threshold, string PriorityText, string StartsAt, string StatusLabel)
{
    public static DefaultAttestationClaimView ToDefaultAttestationClaimView(DefaultAttestationClaim item)
    {
        return new DefaultAttestationClaimView(
            item.Code,
            Text.Truncate(item.Name, 120),
            Text.Truncate(item.Summary, 80),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Timezone,
            Text.Truncate(item.CostCenter, 40),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultAttestationClaimView> ToDefaultAttestationClaimViews(List<DefaultAttestationClaim> items)
    {
        var views = new List<DefaultAttestationClaimView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultAttestationClaimView(item));
        }
        return views;
    }

    public static double SumDefaultAttestationClaimView(DefaultAttestationClaimView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedRate + parsedThreshold;
    }
}
