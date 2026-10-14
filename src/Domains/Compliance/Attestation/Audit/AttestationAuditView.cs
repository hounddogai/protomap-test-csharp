namespace Acme.Workforce.Compliance.Attestation.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a attestation audit.</summary>
public sealed record AttestationAuditView(string Code, string Title, string TimezoneText, string DurationMinutes, string Rate, string CostText, string BudgetText, string CostCenter, string ExternalRef, string StatusLabel)
{
    public static AttestationAuditView ToAttestationAuditView(AttestationAudit item)
    {
        return new AttestationAuditView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 120),
            item.Timezone,
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.ExternalRef,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AttestationAuditView> ToAttestationAuditViews(List<AttestationAudit> items)
    {
        var views = new List<AttestationAuditView>();
        foreach (var item in items)
        {
            views.Add(ToAttestationAuditView(item));
        }
        return views;
    }

    public static double SumAttestationAuditView(AttestationAuditView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + parsedRate + parsedCost;
    }
}
