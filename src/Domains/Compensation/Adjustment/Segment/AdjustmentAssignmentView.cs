namespace Acme.Workforce.Compensation.Adjustment.Segment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adjustment assignment.</summary>
public sealed record AdjustmentAssignmentView(string CodeText, string Name, string Country, string Segment, string AttemptsText, string TimezoneText, string Quantity, string CategoryText, string StartsAt, string StatusLabel)
{
    public static AdjustmentAssignmentView ToAdjustmentAssignmentView(AdjustmentAssignment item)
    {
        return new AdjustmentAssignmentView(
            item.Code,
            Text.Truncate(item.Name, 80),
            item.Country,
            Text.Truncate(item.Segment, 40),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdjustmentAssignmentView> ToAdjustmentAssignmentViews(List<AdjustmentAssignment> items)
    {
        var views = new List<AdjustmentAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToAdjustmentAssignmentView(item));
        }
        return views;
    }

    public static double SumAdjustmentAssignmentView(AdjustmentAssignmentView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedQuantity + (double)parsedStartsAt;
    }
}
