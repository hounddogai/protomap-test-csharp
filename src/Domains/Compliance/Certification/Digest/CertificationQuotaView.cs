namespace Acme.Workforce.Compliance.Certification.Digest;

using System.Globalization;

/// <summary>How screens and exports show a certification quota.</summary>
public sealed record CertificationQuotaView(string Code, string Title, string UpdatedAt, string ExternalRef, string Notes, string ThresholdText, string Budget, string EmployeeIdText, string SequenceText, string StatusLabel)
{
    public static CertificationQuotaView ToCertificationQuotaView(CertificationQuota item)
    {
        return new CertificationQuotaView(
            item.Code,
            item.Title,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Notes,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.EmployeeId,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationQuotaView> ToCertificationQuotaViews(List<CertificationQuota> items)
    {
        var views = new List<CertificationQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationQuotaView(item));
        }
        return views;
    }

    public static double SumCertificationQuotaView(CertificationQuotaView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedThreshold + parsedBudget;
    }
}
