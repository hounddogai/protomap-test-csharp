namespace Acme.Workforce.Compliance.Audit.Plan;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a audit agreement.</summary>
public sealed record AuditAgreementView(string CodeText, string NameText, string ExpiresAt, string ThresholdText, string Country, string CostText, string StatusLabel)
{
    public static AuditAgreementView ToAuditAgreementView(AuditAgreement item)
    {
        return new AuditAgreementView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 120),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AuditAgreementView> ToAuditAgreementViews(List<AuditAgreement> items)
    {
        var views = new List<AuditAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToAuditAgreementView(item));
        }
        return views;
    }

    public static double SumAuditAgreementView(AuditAgreementView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + parsedThreshold + parsedCost;
    }
}
