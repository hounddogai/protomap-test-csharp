namespace Acme.Workforce.Compensation.EquityGrant.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant bundle.</summary>
public sealed record EquityGrantBundleView(string Code, string Label, string ApprovedText, string Limit, string NotesText, string Active, string Retries, string Cost, string SalaryBand, string Name, string StatusLabel)
{
    public static EquityGrantBundleView ToEquityGrantBundleView(EquityGrantBundle item)
    {
        return new EquityGrantBundleView(
            item.Code,
            item.Label,
            $"{item.Approved}",
            item.Limit.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Notes, 40),
            $"{item.Active}",
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 120),
            item.Name,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantBundleView> ToEquityGrantBundleViews(List<EquityGrantBundle> items)
    {
        var views = new List<EquityGrantBundleView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantBundleView(item));
        }
        return views;
    }

    public static double SumEquityGrantBundleView(EquityGrantBundleView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedLimit + (double)parsedRetries + parsedCost;
    }
}
