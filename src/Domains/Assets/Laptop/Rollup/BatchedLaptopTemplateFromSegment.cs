namespace Acme.Workforce.Assets.Laptop.Rollup;

using Acme.Workforce.Assets.AssetReturn.Request;

/// <summary>Functions of batched laptop template from segment.</summary>
public static class BatchedLaptopTemplateFromSegment
{
    public static Dictionary<string, double> SummarizeFallbackAssetReturnSegmentsForTemplate(List<FallbackAssetReturnSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<BatchedLaptopTemplate> DeriveBatchedLaptopTemplatesFromSegment(List<FallbackAssetReturnSegment> items)
    {
        var outValue = new List<BatchedLaptopTemplate>();
        foreach (var item in items)
        {
            var derived = BatchedLaptopTemplate.NewBatchedLaptopTemplate(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankBatchedLaptopTemplate(BatchedLaptopTemplate item)
    {
        return AssetReturnRequestPolicy.NormalizeAssetReturnRequestCode(item.Code);
    }
}
