namespace Acme.Workforce.Orgdesign.Headcount.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a headcount tier.</summary>
public sealed record HeadcountTierView(string Code, string Label, string Capacity, string Fte, string CostCenter, string ContactEmail, string StatusLabel)
{
    public static HeadcountTierView ToHeadcountTierView(HeadcountTier item)
    {
        return new HeadcountTierView(
            Text.Truncate(item.Code, 120),
            item.Label,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.ContactEmail,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<HeadcountTierView> ToHeadcountTierViews(List<HeadcountTier> items)
    {
        var views = new List<HeadcountTierView>();
        foreach (var item in items)
        {
            views.Add(ToHeadcountTierView(item));
        }
        return views;
    }

    public static double SumHeadcountTierView(HeadcountTierView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedFte;
    }
}
