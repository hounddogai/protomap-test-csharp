namespace Acme.Workforce.Payrollops.Payslip.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a payslip forecast.</summary>
public sealed record PayslipForecastView(string Code, string LabelText, string Margin, string JobCode, string WindowDaysText, string Retries, string Capacity, string NameText, string StatusLabel)
{
    public static PayslipForecastView ToPayslipForecastView(PayslipForecast item)
    {
        return new PayslipForecastView(
            item.Code,
            item.Label,
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 64),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayslipForecastView> ToPayslipForecastViews(List<PayslipForecast> items)
    {
        var views = new List<PayslipForecastView>();
        foreach (var item in items)
        {
            views.Add(ToPayslipForecastView(item));
        }
        return views;
    }

    public static double SumPayslipForecastView(PayslipForecastView view)
    {
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return parsedMargin + (double)parsedWindowDays + (double)parsedRetries;
    }
}
