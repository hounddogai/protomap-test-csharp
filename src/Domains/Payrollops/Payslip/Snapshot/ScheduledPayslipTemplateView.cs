namespace Acme.Workforce.Payrollops.Payslip.Snapshot;

using System.Globalization;

/// <summary>How screens and exports show a scheduled payslip template.</summary>
public sealed record ScheduledPayslipTemplateView(string Code, string Label, string Name, string LeaveDays, string GraceDaysText, string Currency, string StatusLabel)
{
    public static ScheduledPayslipTemplateView ToScheduledPayslipTemplateView(ScheduledPayslipTemplate item)
    {
        return new ScheduledPayslipTemplateView(
            item.Code,
            item.Label,
            item.Name,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledPayslipTemplateView> ToScheduledPayslipTemplateViews(List<ScheduledPayslipTemplate> items)
    {
        var views = new List<ScheduledPayslipTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledPayslipTemplateView(item));
        }
        return views;
    }

    public static double SumScheduledPayslipTemplateView(ScheduledPayslipTemplateView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + (double)parsedGraceDays;
    }
}
