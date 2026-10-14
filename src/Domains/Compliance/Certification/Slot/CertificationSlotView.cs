namespace Acme.Workforce.Compliance.Certification.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a certification slot.</summary>
public sealed record CertificationSlotView(string CodeText, string TitleText, string SalaryBand, string EmployeeId, string EndsAt, string ChecksumText, string CostCenterText, string AttemptsText, string TenureMonths, string Revision, string JobCodeText, string StatusLabel)
{
    public static CertificationSlotView ToCertificationSlotView(CertificationSlot item)
    {
        return new CertificationSlotView(
            item.Code,
            item.Title,
            Text.Truncate(item.SalaryBand, 64),
            item.EmployeeId,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Checksum,
            Text.Truncate(item.CostCenter, 40),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.JobCode,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CertificationSlotView> ToCertificationSlotViews(List<CertificationSlot> items)
    {
        var views = new List<CertificationSlotView>();
        foreach (var item in items)
        {
            views.Add(ToCertificationSlotView(item));
        }
        return views;
    }

    public static double SumCertificationSlotView(CertificationSlotView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + (double)parsedAttempts + (double)parsedTenureMonths;
    }
}
