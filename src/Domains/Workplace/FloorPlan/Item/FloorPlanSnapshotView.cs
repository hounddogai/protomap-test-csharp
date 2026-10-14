namespace Acme.Workforce.Workplace.FloorPlan.Item;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan snapshot.</summary>
public sealed record FloorPlanSnapshotView(string Code, string Title, string Maximum, string Source, string Score, string ExternalRefText, string AttemptsText, string Rate, string ContactEmail, string StatusLabel)
{
    public static FloorPlanSnapshotView ToFloorPlanSnapshotView(FloorPlanSnapshot item)
    {
        return new FloorPlanSnapshotView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 80),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.ExternalRef,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanSnapshotView> ToFloorPlanSnapshotViews(List<FloorPlanSnapshot> items)
    {
        var views = new List<FloorPlanSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanSnapshotView(item));
        }
        return views;
    }

    public static double SumFloorPlanSnapshotView(FloorPlanSnapshotView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedScore + (double)parsedAttempts;
    }
}
