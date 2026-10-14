namespace Acme.Workforce.Assets.AssetReturn.Template;

using Acme.Workforce.Assets.Assignment.Exception;

/// <summary>Functions of asset return template from draft.</summary>
public static class AssetReturnTemplateFromDraft
{
    public static Dictionary<string, double> SummarizeSeasonalAssignmentDraftsForTemplate(List<SeasonalAssignmentDraft> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<AssetReturnTemplate> DeriveAssetReturnTemplatesFromDraft(List<SeasonalAssignmentDraft> items)
    {
        var outValue = new List<AssetReturnTemplate>();
        foreach (var item in items)
        {
            var derived = AssetReturnTemplate.NewAssetReturnTemplate(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyAssetReturnTemplate(AssetReturnTemplate item)
    {
        return ManualAssignmentRulePolicy.ManualAssignmentRuleBucket(item.WeightedTotal());
    }
}
