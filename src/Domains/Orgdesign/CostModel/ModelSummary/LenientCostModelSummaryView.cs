namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient cost model summary.</summary>
public sealed record LenientCostModelSummaryView(string Code, string Name, string Archived, string Weight, string Hours, string CeilingText, string Budget, string BadgeText, string CostCenter, string Team, string StatusLabel)
{
    public static LenientCostModelSummaryView ToLenientCostModelSummaryView(LenientCostModelSummary item)
    {
        return new LenientCostModelSummaryView(
            item.Code,
            item.Name,
            $"{item.Archived}",
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Badge,
            Text.Truncate(item.CostCenter, 80),
            item.Team,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientCostModelSummaryView> ToLenientCostModelSummaryViews(List<LenientCostModelSummary> items)
    {
        var views = new List<LenientCostModelSummaryView>();
        foreach (var item in items)
        {
            views.Add(ToLenientCostModelSummaryView(item));
        }
        return views;
    }

    public static double SumLenientCostModelSummaryView(LenientCostModelSummaryView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return parsedWeight + parsedHours + parsedCeiling;
    }
}
