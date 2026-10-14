namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using System.Globalization;

/// <summary>How screens and exports show a fallback payslip digest.</summary>
public sealed record FallbackPayslipDigestView(string CodeText, string Name, string RetriesText, string Ratio, string ExternalRef, string MaximumText, string Skill, string Capacity, string ExpiresAtText, string Limit, string StatusLabel)
{
    public static FallbackPayslipDigestView ToFallbackPayslipDigestView(FallbackPayslipDigest item)
    {
        return new FallbackPayslipDigestView(
            item.Code,
            item.Name,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Skill,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackPayslipDigestView> ToFallbackPayslipDigestViews(List<FallbackPayslipDigest> items)
    {
        var views = new List<FallbackPayslipDigestView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackPayslipDigestView(item));
        }
        return views;
    }

    public static double SumFallbackPayslipDigestView(FallbackPayslipDigestView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return (double)parsedRetries + parsedRatio + parsedMaximum;
    }
}
