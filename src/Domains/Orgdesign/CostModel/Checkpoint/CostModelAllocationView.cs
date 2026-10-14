namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a cost model allocation.</summary>
public sealed record CostModelAllocationView(string CodeText, string Label, string Archived, string JobCode, string Location, string Score, string StatusLabel)
{
    public static CostModelAllocationView ToCostModelAllocationView(CostModelAllocation item)
    {
        return new CostModelAllocationView(
            item.Code,
            item.Label,
            $"{item.Archived}",
            Text.Truncate(item.JobCode, 80),
            item.Location,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CostModelAllocationView> ToCostModelAllocationViews(List<CostModelAllocation> items)
    {
        var views = new List<CostModelAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToCostModelAllocationView(item));
        }
        return views;
    }

    public static double SumCostModelAllocationView(CostModelAllocationView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedScore;
    }
}
