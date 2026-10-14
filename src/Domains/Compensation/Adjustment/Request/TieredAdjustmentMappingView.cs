namespace Acme.Workforce.Compensation.Adjustment.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered adjustment mapping.</summary>
public sealed record TieredAdjustmentMappingView(string CodeText, string Name, string BatchSizeText, string Attempts, string Revision, string Description, string StatusLabel)
{
    public static TieredAdjustmentMappingView ToTieredAdjustmentMappingView(TieredAdjustmentMapping item)
    {
        return new TieredAdjustmentMappingView(
            item.Code,
            item.Name,
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Revision.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredAdjustmentMappingView> ToTieredAdjustmentMappingViews(List<TieredAdjustmentMapping> items)
    {
        var views = new List<TieredAdjustmentMappingView>();
        foreach (var item in items)
        {
            views.Add(ToTieredAdjustmentMappingView(item));
        }
        return views;
    }

    public static double SumTieredAdjustmentMappingView(TieredAdjustmentMappingView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.Attempts, CultureInfo.InvariantCulture);
        var parsedRevision = int.Parse(view.Revision, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + (double)parsedAttempts + (double)parsedRevision;
    }
}
