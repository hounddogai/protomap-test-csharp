namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive pay band group.</summary>
public sealed record AdaptivePayBandGroupView(string CodeText, string Title, string Region, string Approved, string Quantity, string Timezone, string Capacity, string Threshold, string StatusLabel)
{
    public static AdaptivePayBandGroupView ToAdaptivePayBandGroupView(AdaptivePayBandGroup item)
    {
        return new AdaptivePayBandGroupView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Region,
            $"{item.Approved}",
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Timezone,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptivePayBandGroupView> ToAdaptivePayBandGroupViews(List<AdaptivePayBandGroup> items)
    {
        var views = new List<AdaptivePayBandGroupView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptivePayBandGroupView(item));
        }
        return views;
    }

    public static double SumAdaptivePayBandGroupView(AdaptivePayBandGroupView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedCapacity + parsedThreshold;
    }
}
