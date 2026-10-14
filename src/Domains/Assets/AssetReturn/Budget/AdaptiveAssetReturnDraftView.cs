namespace Acme.Workforce.Assets.AssetReturn.Budget;

using System.Globalization;

/// <summary>How screens and exports show a adaptive asset return draft.</summary>
public sealed record AdaptiveAssetReturnDraftView(string Code, string LabelText, string Segment, string CreatedAt, string CapacityText, string BatchSizeText, string CategoryText, string EndsAt, string StatusLabel)
{
    public static AdaptiveAssetReturnDraftView ToAdaptiveAssetReturnDraftView(AdaptiveAssetReturnDraft item)
    {
        return new AdaptiveAssetReturnDraftView(
            item.Code,
            item.Label,
            item.Segment,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Category,
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveAssetReturnDraftView> ToAdaptiveAssetReturnDraftViews(List<AdaptiveAssetReturnDraft> items)
    {
        var views = new List<AdaptiveAssetReturnDraftView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveAssetReturnDraftView(item));
        }
        return views;
    }

    public static double SumAdaptiveAssetReturnDraftView(AdaptiveAssetReturnDraftView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.CapacityText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedCapacity + (double)parsedBatchSize;
    }
}
