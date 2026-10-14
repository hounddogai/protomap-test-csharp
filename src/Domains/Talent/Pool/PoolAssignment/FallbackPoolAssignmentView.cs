namespace Acme.Workforce.Talent.Pool.PoolAssignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback pool assignment.</summary>
public sealed record FallbackPoolAssignmentView(string CodeText, string NameText, string Channel, string Segment, string FloorValue, string Minimum, string StatusLabel)
{
    public static FallbackPoolAssignmentView ToFallbackPoolAssignmentView(FallbackPoolAssignment item)
    {
        return new FallbackPoolAssignmentView(
            item.Code,
            item.Name,
            item.Channel,
            Text.Truncate(item.Segment, 80),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackPoolAssignmentView> ToFallbackPoolAssignmentViews(List<FallbackPoolAssignment> items)
    {
        var views = new List<FallbackPoolAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackPoolAssignmentView(item));
        }
        return views;
    }

    public static double SumFallbackPoolAssignmentView(FallbackPoolAssignmentView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedMinimum;
    }
}
