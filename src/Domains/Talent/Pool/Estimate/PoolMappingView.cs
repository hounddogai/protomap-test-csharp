namespace Acme.Workforce.Talent.Pool.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pool mapping.</summary>
public sealed record PoolMappingView(string Code, string NameText, string Cost, string Revision, string GraceDays, string JobCode, string Threshold, string DiscountText, string CountryText, string Visible, string Label, string StatusLabel)
{
    public static PoolMappingView ToPoolMappingView(PoolMapping item)
    {
        return new PoolMappingView(
            item.Code,
            item.Name,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 64),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Country, 64),
            $"{item.Visible}",
            item.Label,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PoolMappingView> ToPoolMappingViews(List<PoolMapping> items)
    {
        var views = new List<PoolMappingView>();
        foreach (var item in items)
        {
            views.Add(ToPoolMappingView(item));
        }
        return views;
    }

    public static double SumPoolMappingView(PoolMappingView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedRevision + (double)parsedGraceDays;
    }
}
