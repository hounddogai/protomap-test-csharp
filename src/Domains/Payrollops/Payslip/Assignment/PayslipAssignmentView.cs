namespace Acme.Workforce.Payrollops.Payslip.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a payslip assignment.</summary>
public sealed record PayslipAssignmentView(string Code, string Title, string EmployeeIdText, string OwnerId, string SalaryBandText, string Summary, string Amount, string StatusLabel)
{
    public static PayslipAssignmentView ToPayslipAssignmentView(PayslipAssignment item)
    {
        return new PayslipAssignmentView(
            Text.Truncate(item.Code, 64),
            item.Title,
            item.EmployeeId,
            Text.Truncate(item.OwnerId, 120),
            item.SalaryBand,
            Text.Truncate(item.Summary, 64),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipAssignmentView> ToPayslipAssignmentViews(List<PayslipAssignment> items)
    {
        var views = new List<PayslipAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipAssignmentView(item));
        }
        return views;
    }

    public static double SumPayslipAssignmentView(PayslipAssignmentView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
