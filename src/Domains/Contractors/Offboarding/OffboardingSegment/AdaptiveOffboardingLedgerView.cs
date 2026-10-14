namespace Acme.Workforce.Contractors.Offboarding.OffboardingSegment;

using System.Globalization;

/// <summary>How screens and exports show a adaptive offboarding ledger.</summary>
public sealed record AdaptiveOffboardingLedgerView(string CodeText, string Name, string Maximum, string Region, string Skill, string DurationMinutes, string StatusLabel)
{
    public static AdaptiveOffboardingLedgerView ToAdaptiveOffboardingLedgerView(AdaptiveOffboardingLedger item)
    {
        return new AdaptiveOffboardingLedgerView(
            item.Code,
            item.Name,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Region,
            item.Skill,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveOffboardingLedgerView> ToAdaptiveOffboardingLedgerViews(List<AdaptiveOffboardingLedger> items)
    {
        var views = new List<AdaptiveOffboardingLedgerView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveOffboardingLedgerView(item));
        }
        return views;
    }

    public static double SumAdaptiveOffboardingLedgerView(AdaptiveOffboardingLedgerView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedDurationMinutes;
    }
}
