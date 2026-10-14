namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global pay band baseline.</summary>
public sealed record GlobalPayBandBaselineView(string CodeText, string Name, string OwnerId, string UpdatedAt, string Quantity, string StatusLabel)
{
    public static GlobalPayBandBaselineView ToGlobalPayBandBaselineView(GlobalPayBandBaseline item)
    {
        return new GlobalPayBandBaselineView(
            item.Code,
            Text.Truncate(item.Name, 120),
            item.OwnerId,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalPayBandBaselineView> ToGlobalPayBandBaselineViews(List<GlobalPayBandBaseline> items)
    {
        var views = new List<GlobalPayBandBaselineView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalPayBandBaselineView(item));
        }
        return views;
    }

    public static double SumGlobalPayBandBaselineView(GlobalPayBandBaselineView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedQuantity;
    }
}
