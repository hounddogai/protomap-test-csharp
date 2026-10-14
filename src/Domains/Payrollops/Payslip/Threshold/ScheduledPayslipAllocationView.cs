namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled payslip allocation.</summary>
public sealed record ScheduledPayslipAllocationView(string Code, string LabelText, string LeaveDays, string FteText, string Title, string GradeText, string StatusLabel)
{
    public static ScheduledPayslipAllocationView ToScheduledPayslipAllocationView(ScheduledPayslipAllocation item)
    {
        return new ScheduledPayslipAllocationView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            item.Grade,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledPayslipAllocationView> ToScheduledPayslipAllocationViews(List<ScheduledPayslipAllocation> items)
    {
        var views = new List<ScheduledPayslipAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledPayslipAllocationView(item));
        }
        return views;
    }

    public static double SumScheduledPayslipAllocationView(ScheduledPayslipAllocationView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.FteText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedFte;
    }
}
