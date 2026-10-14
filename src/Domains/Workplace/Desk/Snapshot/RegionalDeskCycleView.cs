namespace Acme.Workforce.Workplace.Desk.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional desk cycle.</summary>
public sealed record RegionalDeskCycleView(string Code, string Name, string Limit, string CostText, string LocationText, string Ratio, string StatusLabel)
{
    public static RegionalDeskCycleView ToRegionalDeskCycleView(RegionalDeskCycle item)
    {
        return new RegionalDeskCycleView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalDeskCycleView> ToRegionalDeskCycleViews(List<RegionalDeskCycle> items)
    {
        var views = new List<RegionalDeskCycleView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalDeskCycleView(item));
        }
        return views;
    }

    public static double SumRegionalDeskCycleView(RegionalDeskCycleView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedCost + parsedRatio;
    }
}
