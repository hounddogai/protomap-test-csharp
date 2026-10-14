namespace Acme.Workforce.Compliance.Attestation.Template;

using System.Globalization;

/// <summary>How screens and exports show a capped attestation audit.</summary>
public sealed record CappedAttestationAuditView(string Code, string Label, string Retries, string LeaveDays, string ManagerIdText, string Threshold, string Description, string Maximum, string Ceiling, string WindowDaysText, string StatusLabel)
{
    public static CappedAttestationAuditView ToCappedAttestationAuditView(CappedAttestationAudit item)
    {
        return new CappedAttestationAuditView(
            item.Code,
            item.Label,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Description,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedAttestationAuditView> ToCappedAttestationAuditViews(List<CappedAttestationAudit> items)
    {
        var views = new List<CappedAttestationAuditView>();
        foreach (var item in items)
        {
            views.Add(ToCappedAttestationAuditView(item));
        }
        return views;
    }

    public static double SumCappedAttestationAuditView(CappedAttestationAuditView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedLeaveDays + parsedThreshold;
    }
}
