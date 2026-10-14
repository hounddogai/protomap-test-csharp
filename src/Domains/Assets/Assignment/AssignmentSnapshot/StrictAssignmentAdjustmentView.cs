namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a strict assignment adjustment.</summary>
public sealed record StrictAssignmentAdjustmentView(string Code, string Label, string Grade, string Name, string Description, string FloorValue, string Phone, string StartsAtText, string Threshold, string RatingText, string StatusLabel)
{
    public static StrictAssignmentAdjustmentView ToStrictAssignmentAdjustmentView(StrictAssignmentAdjustment item)
    {
        return new StrictAssignmentAdjustmentView(
            item.Code,
            item.Label,
            Text.Truncate(item.Grade, 40),
            item.Name,
            item.Description,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictAssignmentAdjustmentView> ToStrictAssignmentAdjustmentViews(List<StrictAssignmentAdjustment> items)
    {
        var views = new List<StrictAssignmentAdjustmentView>();
        foreach (var item in items)
        {
            views.Add(ToStrictAssignmentAdjustmentView(item));
        }
        return views;
    }

    public static double SumStrictAssignmentAdjustmentView(StrictAssignmentAdjustmentView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAtText, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedStartsAt + parsedThreshold;
    }
}
