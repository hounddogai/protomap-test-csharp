namespace Acme.Workforce.Orgdesign.Position.Estimate;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered position policy.</summary>
public sealed record TieredPositionPolicyView(string Code, string Title, string Description, string Capacity, string BatchSize, string StatusLabel)
{
    public static TieredPositionPolicyView ToTieredPositionPolicyView(TieredPositionPolicy item)
    {
        return new TieredPositionPolicyView(
            item.Code,
            item.Title,
            Text.Truncate(item.Description, 80),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredPositionPolicyView> ToTieredPositionPolicyViews(List<TieredPositionPolicy> items)
    {
        var views = new List<TieredPositionPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToTieredPositionPolicyView(item));
        }
        return views;
    }

    public static double SumTieredPositionPolicyView(TieredPositionPolicyView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + (double)parsedBatchSize;
    }
}
