namespace Acme.Workforce.Orgdesign.Position.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional position policy.</summary>
public sealed record RegionalPositionPolicyView(string Code, string Label, string BatchSize, string CreatedAtText, string Fte, string Discount, string Rating, string Checksum, string StatusLabel)
{
    public static RegionalPositionPolicyView ToRegionalPositionPolicyView(RegionalPositionPolicy item)
    {
        return new RegionalPositionPolicyView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Label, 80),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalPositionPolicyView> ToRegionalPositionPolicyViews(List<RegionalPositionPolicy> items)
    {
        var views = new List<RegionalPositionPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalPositionPolicyView(item));
        }
        return views;
    }

    public static double SumRegionalPositionPolicyView(RegionalPositionPolicyView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAtText, CultureInfo.InvariantCulture);
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedCreatedAt + parsedFte;
    }
}
