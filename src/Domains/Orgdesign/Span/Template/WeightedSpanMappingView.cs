namespace Acme.Workforce.Orgdesign.Span.Template;

using System.Globalization;

/// <summary>How screens and exports show a weighted span mapping.</summary>
public sealed record WeightedSpanMappingView(string CodeText, string Name, string SalaryBandText, string Capacity, string Pinned, string DepartmentText, string FloorValueText, string Source, string StatusLabel)
{
    public static WeightedSpanMappingView ToWeightedSpanMappingView(WeightedSpanMapping item)
    {
        return new WeightedSpanMappingView(
            item.Code,
            item.Name,
            item.SalaryBand,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Department,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.Source,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedSpanMappingView> ToWeightedSpanMappingViews(List<WeightedSpanMapping> items)
    {
        var views = new List<WeightedSpanMappingView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedSpanMappingView(item));
        }
        return views;
    }

    public static double SumWeightedSpanMappingView(WeightedSpanMappingView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedFloorValue;
    }
}
