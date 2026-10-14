namespace Acme.Workforce.Orgdesign.Position.Mapping;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a position mapping.</summary>
public sealed record PositionMappingView(string Code, string Title, string Version, string Active, string Capacity, string StatusLabel)
{
    public static PositionMappingView ToPositionMappingView(PositionMapping item)
    {
        return new PositionMappingView(
            Text.Truncate(item.Code, 40),
            item.Title,
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PositionMappingView> ToPositionMappingViews(List<PositionMapping> items)
    {
        var views = new List<PositionMappingView>();
        foreach (var item in items)
        {
            views.Add(ToPositionMappingView(item));
        }
        return views;
    }

    public static double SumPositionMappingView(PositionMappingView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedCapacity;
    }
}
