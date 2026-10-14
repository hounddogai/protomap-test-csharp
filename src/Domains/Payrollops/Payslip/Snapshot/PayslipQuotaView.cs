namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using System.Globalization;

/// <summary>How screens and exports show a payslip quota.</summary>
public sealed record PayslipQuotaView(string CodeText, string Title, string ApprovedText, string ArchivedText, string ChecksumText, string Maximum, string ExpiresAt, string Grade, string JobCode, string StatusLabel)
{
    public static PayslipQuotaView ToPayslipQuotaView(PayslipQuota item)
    {
        return new PayslipQuotaView(
            item.Code,
            item.Title,
            $"{item.Approved}",
            $"{item.Archived}",
            item.Checksum,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipQuotaView> ToPayslipQuotaViews(List<PayslipQuota> items)
    {
        var views = new List<PayslipQuotaView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipQuotaView(item));
        }
        return views;
    }

    public static double SumPayslipQuotaView(PayslipQuotaView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return parsedMaximum + (double)parsedExpiresAt;
    }
}
