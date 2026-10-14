namespace Acme.Workforce.Payrollops.Payslip.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a payslip estimate.</summary>
public sealed record PayslipEstimateView(string Code, string Name, string ExpiresAt, string DurationMinutesText, string Timezone, string Channel, string Active, string SummaryText, string StatusLabel)
{
    public static PayslipEstimateView ToPayslipEstimateView(PayslipEstimate item)
    {
        return new PayslipEstimateView(
            item.Code,
            item.Name,
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 40),
            item.Channel,
            $"{item.Active}",
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipEstimateView> ToPayslipEstimateViews(List<PayslipEstimate> items)
    {
        var views = new List<PayslipEstimateView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipEstimateView(item));
        }
        return views;
    }

    public static double SumPayslipEstimateView(PayslipEstimateView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedDurationMinutes;
    }
}
