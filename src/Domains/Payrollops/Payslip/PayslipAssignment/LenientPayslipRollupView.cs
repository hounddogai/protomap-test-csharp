namespace Acme.Workforce.Payrollops.Payslip.PayslipAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient payslip rollup.</summary>
public sealed record LenientPayslipRollupView(string Code, string Title, string Threshold, string Name, string Minimum, string LeaveDaysText, string ExternalRefText, string JobCode, string StatusLabel)
{
    public static LenientPayslipRollupView ToLenientPayslipRollupView(LenientPayslipRollup item)
    {
        return new LenientPayslipRollupView(
            item.Code,
            item.Title,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 40),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ExternalRef, 64),
            Text.Truncate(item.JobCode, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientPayslipRollupView> ToLenientPayslipRollupViews(List<LenientPayslipRollup> items)
    {
        var views = new List<LenientPayslipRollupView>();
        foreach (var item in items)
        {
            views.Add(ToLenientPayslipRollupView(item));
        }
        return views;
    }

    public static double SumLenientPayslipRollupView(LenientPayslipRollupView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedMinimum + (double)parsedLeaveDays;
    }
}
