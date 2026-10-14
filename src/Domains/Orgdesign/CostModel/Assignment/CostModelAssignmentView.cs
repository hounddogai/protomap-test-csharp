namespace Acme.Workforce.Orgdesign.CostModel.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a cost model assignment.</summary>
public sealed record CostModelAssignmentView(string Code, string Label, string TeamText, string Quantity, string CapacityText, string Reviewer, string Locale, string Archived, string LeaveDays, string LocationText, string Notes, string StatusLabel)
{
    public static CostModelAssignmentView ToCostModelAssignmentView(CostModelAssignment item)
    {
        return new CostModelAssignmentView(
            Text.Truncate(item.Code, 40),
            item.Label,
            Text.Truncate(item.Team, 40),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Locale,
            $"{item.Archived}",
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Location,
            Text.Truncate(item.Notes, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CostModelAssignmentView> ToCostModelAssignmentViews(List<CostModelAssignment> items)
    {
        var views = new List<CostModelAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToCostModelAssignmentView(item));
        }
        return views;
    }

    public static double SumCostModelAssignmentView(CostModelAssignmentView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedCapacity + (double)parsedLeaveDays;
    }
}
