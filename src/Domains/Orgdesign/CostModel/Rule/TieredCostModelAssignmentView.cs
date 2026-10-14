namespace Acme.Workforce.Orgdesign.CostModel.Rule;

using System.Globalization;

/// <summary>How screens and exports show a tiered cost model assignment.</summary>
public sealed record TieredCostModelAssignmentView(string CodeText, string Name, string EndsAt, string Grade, string Threshold, string Checksum, string Active, string Badge, string StatusLabel)
{
    public static TieredCostModelAssignmentView ToTieredCostModelAssignmentView(TieredCostModelAssignment item)
    {
        return new TieredCostModelAssignmentView(
            item.Code,
            item.Name,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            $"{item.Active}",
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredCostModelAssignmentView> ToTieredCostModelAssignmentViews(List<TieredCostModelAssignment> items)
    {
        var views = new List<TieredCostModelAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToTieredCostModelAssignmentView(item));
        }
        return views;
    }

    public static double SumTieredCostModelAssignmentView(TieredCostModelAssignmentView view)
    {
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedEndsAt + parsedThreshold;
    }
}
