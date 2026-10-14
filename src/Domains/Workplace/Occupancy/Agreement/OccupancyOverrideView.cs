namespace Acme.Workforce.Workplace.Occupancy.Agreement;

using System.Globalization;

/// <summary>How screens and exports show a occupancy override.</summary>
public sealed record OccupancyOverrideView(string Code, string NameText, string Revision, string RatioText, string Archived, string StatusLabel)
{
    public static OccupancyOverrideView ToOccupancyOverrideView(OccupancyOverride item)
    {
        return new OccupancyOverrideView(
            item.Code,
            item.Name,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OccupancyOverrideView> ToOccupancyOverrideViews(List<OccupancyOverride> items)
    {
        var views = new List<OccupancyOverrideView>();
        foreach (var item in items)
        {
            views.Add(ToOccupancyOverrideView(item));
        }
        return views;
    }

    public static double SumOccupancyOverrideView(OccupancyOverrideView view)
    {
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.RatioText, CultureInfo.InvariantCulture);
        return (double)parsedRevision + parsedRatio;
    }
}
