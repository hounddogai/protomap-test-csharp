namespace Acme.Workforce.Compliance.Certification.CertificationAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped certification assignment.</summary>
public sealed record CappedCertificationAssignmentView(string Code, string TitleText, string Retries, string Attempts, string Ratio, string Priority, string SalaryBand, string Discount, string ExpiresAtText, string Locale, string Unit, string StatusLabel)
{
    public static CappedCertificationAssignmentView ToCappedCertificationAssignmentView(CappedCertificationAssignment item)
    {
        return new CappedCertificationAssignmentView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Title, 120),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedCertificationAssignmentView> ToCappedCertificationAssignmentViews(List<CappedCertificationAssignment> items)
    {
        var views = new List<CappedCertificationAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToCappedCertificationAssignmentView(item));
        }
        return views;
    }

    public static double SumCappedCertificationAssignmentView(CappedCertificationAssignmentView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedAttempts + parsedRatio;
    }
}
