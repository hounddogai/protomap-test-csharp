namespace Acme.Workforce.Orgdesign.Scenario.Metric;

using System.Globalization;

/// <summary>How screens and exports show a pending scenario agreement.</summary>
public sealed record PendingScenarioAgreementView(string CodeText, string Label, string ExpiresAt, string ExternalRefText, string CapacityText, string Locale, string ManagerId, string StatusLabel)
{
    public static PendingScenarioAgreementView ToPendingScenarioAgreementView(PendingScenarioAgreement item)
    {
        return new PendingScenarioAgreementView(
            item.Code,
            item.Label,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            item.ManagerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingScenarioAgreementView> ToPendingScenarioAgreementViews(List<PendingScenarioAgreement> items)
    {
        var views = new List<PendingScenarioAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToPendingScenarioAgreementView(item));
        }
        return views;
    }

    public static double SumPendingScenarioAgreementView(PendingScenarioAgreementView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedCapacity;
    }
}
