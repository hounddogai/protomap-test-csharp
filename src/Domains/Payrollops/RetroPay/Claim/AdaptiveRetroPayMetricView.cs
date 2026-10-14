namespace Acme.Workforce.Payrollops.RetroPay.Claim;

using System.Globalization;

/// <summary>How screens and exports show a adaptive retro pay metric.</summary>
public sealed record AdaptiveRetroPayMetricView(string Code, string Title, string BudgetText, string LeaveDays, string Amount, string Hours, string StatusLabel)
{
    public static AdaptiveRetroPayMetricView ToAdaptiveRetroPayMetricView(AdaptiveRetroPayMetric item)
    {
        return new AdaptiveRetroPayMetricView(
            item.Code,
            item.Title,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveRetroPayMetricView> ToAdaptiveRetroPayMetricViews(List<AdaptiveRetroPayMetric> items)
    {
        var views = new List<AdaptiveRetroPayMetricView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveRetroPayMetricView(item));
        }
        return views;
    }

    public static double SumAdaptiveRetroPayMetricView(AdaptiveRetroPayMetricView view)
    {
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        return parsedBudget + (double)parsedLeaveDays + parsedAmount;
    }
}
