namespace Acme.Workforce.Payrollops.PayRun.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay run threshold.</summary>
public sealed record PayRunThresholdView(string Code, string NameText, string CountryText, string Active, string Threshold, string CostText, string ExternalRef, string ManagerId, string SalaryBand, string LocationText, string StatusLabel)
{
    public static PayRunThresholdView ToPayRunThresholdView(PayRunThreshold item)
    {
        return new PayRunThresholdView(
            item.Code,
            item.Name,
            Text.Truncate(item.Country, 120),
            $"{item.Active}",
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.ManagerId,
            item.SalaryBand,
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunThresholdView> ToPayRunThresholdViews(List<PayRunThreshold> items)
    {
        var views = new List<PayRunThresholdView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunThresholdView(item));
        }
        return views;
    }

    public static double SumPayRunThresholdView(PayRunThresholdView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return parsedThreshold + parsedCost;
    }
}
