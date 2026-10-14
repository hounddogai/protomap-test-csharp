namespace Acme.Workforce.Workplace.FloorPlan.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a seasonal floor plan record.</summary>
public sealed record SeasonalFloorPlanRecordView(string Code, string LabelText, string Unit, string DescriptionText, string StartsAt, string Approved, string Location, string Badge, string StatusLabel)
{
    public static SeasonalFloorPlanRecordView ToSeasonalFloorPlanRecordView(SeasonalFloorPlanRecord item)
    {
        return new SeasonalFloorPlanRecordView(
            Text.Truncate(item.Code, 80),
            item.Label,
            item.Unit,
            Text.Truncate(item.Description, 80),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            Text.Truncate(item.Location, 64),
            item.Badge,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SeasonalFloorPlanRecordView> ToSeasonalFloorPlanRecordViews(List<SeasonalFloorPlanRecord> items)
    {
        var views = new List<SeasonalFloorPlanRecordView>();
        foreach (var item in items)
        {
            views.Add(ToSeasonalFloorPlanRecordView(item));
        }
        return views;
    }

    public static double SumSeasonalFloorPlanRecordView(SeasonalFloorPlanRecordView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt;
    }
}
