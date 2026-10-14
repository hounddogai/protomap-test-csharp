namespace Acme.Workforce.Workplace.FloorPlan.Slot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a floor plan digest.</summary>
public sealed record FloorPlanDigestView(string Code, string Label, string Notes, string Priority, string Description, string Unit, string Retries, string CategoryText, string StatusLabel)
{
    public static FloorPlanDigestView ToFloorPlanDigestView(FloorPlanDigest item)
    {
        return new FloorPlanDigestView(
            item.Code,
            item.Label,
            Text.Truncate(item.Notes, 64),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 40),
            Text.Truncate(item.Unit, 64),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Category,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FloorPlanDigestView> ToFloorPlanDigestViews(List<FloorPlanDigest> items)
    {
        var views = new List<FloorPlanDigestView>();
        foreach (var item in items)
        {
            views.Add(ToFloorPlanDigestView(item));
        }
        return views;
    }

    public static double SumFloorPlanDigestView(FloorPlanDigestView view)
    {
        var parsedPriority = int.Parse(view.Priority, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedRetries;
    }
}
