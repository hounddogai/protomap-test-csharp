namespace Acme.Workforce.Talent.Mobility.Target;

using System.Globalization;

/// <summary>How screens and exports show a mobility allocation.</summary>
public sealed record MobilityAllocationView(string Code, string Name, string Capacity, string Margin, string LimitText, string UpdatedAtText, string StatusLabel)
{
    public static MobilityAllocationView ToMobilityAllocationView(MobilityAllocation item)
    {
        return new MobilityAllocationView(
            item.Code,
            item.Name,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityAllocationView> ToMobilityAllocationViews(List<MobilityAllocation> items)
    {
        var views = new List<MobilityAllocationView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityAllocationView(item));
        }
        return views;
    }

    public static double SumMobilityAllocationView(MobilityAllocationView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedMargin + (double)parsedLimit;
    }
}
