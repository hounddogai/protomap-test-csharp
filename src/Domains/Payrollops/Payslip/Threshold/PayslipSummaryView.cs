namespace Acme.Workforce.Payrollops.Payslip.Threshold;

using System.Globalization;

/// <summary>How screens and exports show a payslip summary.</summary>
public sealed record PayslipSummaryView(string Code, string Name, string Title, string Channel, string Attempts, string OwnerIdText, string RateText, string Archived, string DepartmentText, string StatusLabel)
{
    public static PayslipSummaryView ToPayslipSummaryView(PayslipSummary item)
    {
        return new PayslipSummaryView(
            item.Code,
            item.Name,
            item.Title,
            item.Channel,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Department,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipSummaryView> ToPayslipSummaryViews(List<PayslipSummary> items)
    {
        var views = new List<PayslipSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipSummaryView(item));
        }
        return views;
    }

    public static double SumPayslipSummaryView(PayslipSummaryView view)
    {
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedRate;
    }
}
