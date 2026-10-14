namespace Acme.Workforce.Payrollops.Payslip.PayslipDigest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy payslip digest.</summary>
public sealed record LegacyPayslipDigestView(string Code, string Title, string ReviewerText, string OwnerId, string ManagerId, string DurationMinutesText, string GraceDays, string StatusLabel)
{
    public static LegacyPayslipDigestView ToLegacyPayslipDigestView(LegacyPayslipDigest item)
    {
        return new LegacyPayslipDigestView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Reviewer,
            Text.Truncate(item.OwnerId, 80),
            Text.Truncate(item.ManagerId, 40),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyPayslipDigestView> ToLegacyPayslipDigestViews(List<LegacyPayslipDigest> items)
    {
        var views = new List<LegacyPayslipDigestView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyPayslipDigestView(item));
        }
        return views;
    }

    public static double SumLegacyPayslipDigestView(LegacyPayslipDigestView view)
    {
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedDurationMinutes + (double)parsedGraceDays;
    }
}
