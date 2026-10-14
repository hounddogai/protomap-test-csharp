namespace Acme.Workforce.Payrollops.Payslip.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual payslip allocation.</summary>
public sealed record ManualPayslipAllocationView(string Code, string Name, string ScoreText, string GraceDaysText, string CountryText, string Phone, string Revision, string Threshold, string TenureMonths, string Department, string StatusLabel)
{
    public static ManualPayslipAllocationView ToManualPayslipAllocationView(ManualPayslipAllocation item)
    {
        return new ManualPayslipAllocationView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 120),
            item.Phone,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualPayslipAllocationView> ToManualPayslipAllocationViews(List<ManualPayslipAllocation> items)
    {
        var views = new List<ManualPayslipAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToManualPayslipAllocationView(item));
        }
        return views;
    }

    public static double SumManualPayslipAllocationView(ManualPayslipAllocationView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedGraceDays + (double)parsedRevision;
    }
}
