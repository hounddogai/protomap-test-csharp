namespace Acme.Workforce.Orgdesign.Position.Plan;

using System.Globalization;

/// <summary>How screens and exports show a rolling position manifest.</summary>
public sealed record RollingPositionManifestView(string Code, string Name, string FloorValue, string Segment, string Reviewer, string PriorityText, string StatusLabel)
{
    public static RollingPositionManifestView ToRollingPositionManifestView(RollingPositionManifest item)
    {
        return new RollingPositionManifestView(
            item.Code,
            item.Name,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Segment,
            item.Reviewer,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingPositionManifestView> ToRollingPositionManifestViews(List<RollingPositionManifest> items)
    {
        var views = new List<RollingPositionManifestView>();
        foreach (var item in items)
        {
            views.Add(ToRollingPositionManifestView(item));
        }
        return views;
    }

    public static double SumRollingPositionManifestView(RollingPositionManifestView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedPriority;
    }
}
