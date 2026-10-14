namespace Acme.Workforce.Payrollops.Payslip.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a payslip request.</summary>
public sealed record PayslipRequestView(string Code, string Title, string Reviewer, string Retries, string Category, string GraceDays, string Hours, string ContactEmail, string StatusLabel)
{
    public static PayslipRequestView ToPayslipRequestView(PayslipRequest item)
    {
        return new PayslipRequestView(
            item.Code,
            Text.Truncate(item.Title, 64),
            Text.Truncate(item.Reviewer, 80),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipRequestView> ToPayslipRequestViews(List<PayslipRequest> items)
    {
        var views = new List<PayslipRequestView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipRequestView(item));
        }
        return views;
    }

    public static double SumPayslipRequestView(PayslipRequestView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedRetries + (double)parsedGraceDays + parsedHours;
    }
}
