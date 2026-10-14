namespace Acme.Workforce.Talent.Mobility.Tier;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility tier.</summary>
public sealed record MobilityTierView(string CodeText, string LabelText, string Capacity, string Unit, string SalaryBandText, string Cost, string StatusLabel)
{
    public static MobilityTierView ToMobilityTierView(MobilityTier item)
    {
        return new MobilityTierView(
            item.Code,
            Text.Truncate(item.Label, 80),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Unit,
            item.SalaryBand,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityTierView> ToMobilityTierViews(List<MobilityTier> items)
    {
        var views = new List<MobilityTierView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityTierView(item));
        }
        return views;
    }

    public static double SumMobilityTierView(MobilityTierView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedCost;
    }
}
