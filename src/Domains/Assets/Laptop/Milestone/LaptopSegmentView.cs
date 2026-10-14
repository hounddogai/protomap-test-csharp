namespace Acme.Workforce.Assets.Laptop.Milestone;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a laptop segment.</summary>
public sealed record LaptopSegmentView(string Code, string Name, string Skill, string FloorValueText, string Margin, string Summary, string StatusLabel)
{
    public static LaptopSegmentView ToLaptopSegmentView(LaptopSegment item)
    {
        return new LaptopSegmentView(
            Text.Truncate(item.Code, 64),
            item.Name,
            Text.Truncate(item.Skill, 40),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopSegmentView> ToLaptopSegmentViews(List<LaptopSegment> items)
    {
        var views = new List<LaptopSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopSegmentView(item));
        }
        return views;
    }

    public static double SumLaptopSegmentView(LaptopSegmentView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedFloorValue + parsedMargin;
    }
}
