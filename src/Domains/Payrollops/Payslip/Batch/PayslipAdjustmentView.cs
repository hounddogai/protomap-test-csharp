namespace Acme.Workforce.Payrollops.Payslip.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a payslip adjustment.</summary>
public sealed record PayslipAdjustmentView(string Code, string Name, string Weight, string Visible, string Score, string Attempts, string OwnerIdText, string StatusLabel)
{
    public static PayslipAdjustmentView ToPayslipAdjustmentView(PayslipAdjustment item)
    {
        return new PayslipAdjustmentView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipAdjustmentView> ToPayslipAdjustmentViews(List<PayslipAdjustment> items)
    {
        var views = new List<PayslipAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipAdjustmentView(item));
        }
        return views;
    }

    public static double SumPayslipAdjustmentView(PayslipAdjustmentView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        return parsedWeight + parsedScore + (double)parsedAttempts;
    }
}
