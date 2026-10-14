namespace Acme.Workforce.Payrollops.Payslip.Profile;

using System.Globalization;

/// <summary>How screens and exports show a payslip schedule.</summary>
public sealed record PayslipScheduleView(string Code, string LabelText, string Cost, string LocationText, string AmountText, string StatusLabel)
{
    public static PayslipScheduleView ToPayslipScheduleView(PayslipSchedule item)
    {
        return new PayslipScheduleView(
            item.Code,
            item.Label,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipScheduleView> ToPayslipScheduleViews(List<PayslipSchedule> items)
    {
        var views = new List<PayslipScheduleView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipScheduleView(item));
        }
        return views;
    }

    public static double SumPayslipScheduleView(PayslipScheduleView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedCost + parsedAmount;
    }
}
