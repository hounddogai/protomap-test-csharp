namespace Acme.Workforce.Payrollops.Payslip.PayslipSnapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a payslip batch.</summary>
public sealed record PayslipBatchView(string CodeText, string TitleText, string Notes, string Name, string ManagerId, string Country, string Score, string StatusLabel)
{
    public static PayslipBatchView ToPayslipBatchView(PayslipBatch item)
    {
        return new PayslipBatchView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Notes,
            item.Name,
            Text.Truncate(item.ManagerId, 64),
            item.Country,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipBatchView> ToPayslipBatchViews(List<PayslipBatch> items)
    {
        var views = new List<PayslipBatchView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipBatchView(item));
        }
        return views;
    }

    public static double SumPayslipBatchView(PayslipBatchView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
