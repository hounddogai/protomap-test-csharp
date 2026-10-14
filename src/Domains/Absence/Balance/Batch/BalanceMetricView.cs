namespace Acme.Workforce.Absence.Balance.Batch;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a balance metric.</summary>
public sealed record BalanceMetricView(string Code, string Title, string ManagerId, string Minimum, string Country, string StatusLabel)
{
    public static BalanceMetricView ToBalanceMetricView(BalanceMetric item)
    {
        return new BalanceMetricView(
            item.Code,
            item.Title,
            item.ManagerId,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BalanceMetricView> ToBalanceMetricViews(List<BalanceMetric> items)
    {
        var views = new List<BalanceMetricView>();
        foreach (var item in items)
        {
            views.Add(ToBalanceMetricView(item));
        }
        return views;
    }

    public static double SumBalanceMetricView(BalanceMetricView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedMinimum;
    }
}
