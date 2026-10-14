namespace Acme.Workforce.Assets.Laptop.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a laptop mapping.</summary>
public sealed record LaptopMappingView(string Code, string NameText, string Score, string PinnedText, string Department, string Margin, string StatusLabel)
{
    public static LaptopMappingView ToLaptopMappingView(LaptopMapping item)
    {
        return new LaptopMappingView(
            item.Code,
            item.Name,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            Text.Truncate(item.Department, 80),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LaptopMappingView> ToLaptopMappingViews(List<LaptopMapping> items)
    {
        var views = new List<LaptopMappingView>();
        foreach (var item in items)
        {
            views.Add(ToLaptopMappingView(item));
        }
        return views;
    }

    public static double SumLaptopMappingView(LaptopMappingView view)
    {
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return parsedScore + parsedMargin;
    }
}
