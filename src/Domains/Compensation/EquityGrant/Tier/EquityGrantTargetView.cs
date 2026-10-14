namespace Acme.Workforce.Compensation.EquityGrant.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a equity grant target.</summary>
public sealed record EquityGrantTargetView(string Code, string Label, string BatchSize, string CostCenter, string Unit, string Currency, string StatusLabel)
{
    public static EquityGrantTargetView ToEquityGrantTargetView(EquityGrantTarget item)
    {
        return new EquityGrantTargetView(
            item.Code,
            item.Label,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.CostCenter, 64),
            item.Unit,
            item.Currency,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<EquityGrantTargetView> ToEquityGrantTargetViews(List<EquityGrantTarget> items)
    {
        var views = new List<EquityGrantTargetView>();
        foreach (var item in items)
        {
            views.Add(ToEquityGrantTargetView(item));
        }
        return views;
    }

    public static double SumEquityGrantTargetView(EquityGrantTargetView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize;
    }
}
