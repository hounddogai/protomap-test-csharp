namespace Acme.Workforce.Workplace.FloorPlan.Notice;

using System.Globalization;

/// <summary>How screens and exports show a default floor plan target.</summary>
public sealed record DefaultFloorPlanTargetView(string Code, string Label, string Name, string Approved, string StartsAt, string ArchivedText, string ReviewerText, string Sequence, string StatusLabel)
{
    public static DefaultFloorPlanTargetView ToDefaultFloorPlanTargetView(DefaultFloorPlanTarget item)
    {
        return new DefaultFloorPlanTargetView(
            item.Code,
            item.Label,
            item.Name,
            $"{item.Approved}",
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Reviewer,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultFloorPlanTargetView> ToDefaultFloorPlanTargetViews(List<DefaultFloorPlanTarget> items)
    {
        var views = new List<DefaultFloorPlanTargetView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultFloorPlanTargetView(item));
        }
        return views;
    }

    public static double SumDefaultFloorPlanTargetView(DefaultFloorPlanTargetView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt + (double)parsedSequence;
    }
}
