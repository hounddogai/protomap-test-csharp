namespace Acme.Workforce.Payrollops.Payslip.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a payslip allocation.</summary>
public sealed record PayslipAllocationView(string Code, string Title, string Active, string Threshold, string BadgeText, string StatusLabel)
{
    public static PayslipAllocationView ToPayslipAllocationView(PayslipAllocation item)
    {
        return new PayslipAllocationView(
            item.Code,
            item.Title,
            $"{item.Active}",
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Badge, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipAllocationView> ToPayslipAllocationViews(List<PayslipAllocation> items)
    {
        var views = new List<PayslipAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipAllocationView(item));
        }
        return views;
    }

    public static double SumPayslipAllocationView(PayslipAllocationView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedThreshold;
    }
}
