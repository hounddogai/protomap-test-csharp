namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped attestation quota.</summary>
public sealed record CappedAttestationQuotaView(string Code, string Label, string Segment, string SalaryBand, string Quantity, string UpdatedAt, string Threshold, string StartsAt, string Skill, string LeaveDays, string Region, string StatusLabel)
{
    public static CappedAttestationQuotaView ToCappedAttestationQuotaView(CappedAttestationQuota item)
    {
        return new CappedAttestationQuotaView(
            item.Code,
            item.Label,
            Text.Truncate(item.Segment, 40),
            Text.Truncate(item.SalaryBand, 40),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Region, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedAttestationQuotaView> ToCappedAttestationQuotaViews(List<CappedAttestationQuota> items)
    {
        var views = new List<CappedAttestationQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToCappedAttestationQuotaView(item));
        }
        return views;
    }

    public static double SumCappedAttestationQuotaView(CappedAttestationQuotaView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedUpdatedAt + parsedThreshold;
    }
}
