namespace Acme.Workforce.Assets.AssetReturn.Cycle;

using Acme.Workforce.Assets.Assignment.AssignmentEntry;

/// <summary>Functions of adaptive asset return notice from entry.</summary>
public static class AdaptiveAssetReturnNoticeFromEntry
{
    public static Dictionary<string, double> SummarizePendingAssignmentEntriesForNotice(List<PendingAssignmentEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<AdaptiveAssetReturnNotice> DeriveAdaptiveAssetReturnNoticesFromEntry(List<PendingAssignmentEntry> items)
    {
        var outValue = new List<AdaptiveAssetReturnNotice>();
        foreach (var item in items)
        {
            var derived = AdaptiveAssetReturnNotice.NewAdaptiveAssetReturnNotice(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeAdaptiveAssetReturnNotice(AdaptiveAssetReturnNotice item)
    {
        return AssignmentExceptionPolicy.NormalizeAssignmentExceptionCode(item.Code);
    }
}
