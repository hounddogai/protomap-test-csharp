namespace Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

using Acme.Workforce.Assets.AssetReturn.Batch;

/// <summary>Functions of adaptive license seat notice from assignment.</summary>
public static class AdaptiveLicenseSeatNoticeFromAssignment
{
    public static Dictionary<string, double> SummarizeCappedAssetReturnAssignmentsForNotice(List<CappedAssetReturnAssignment> items)
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

    public static List<AdaptiveLicenseSeatNotice> DeriveAdaptiveLicenseSeatNoticesFromAssignment(List<CappedAssetReturnAssignment> items)
    {
        var outValue = new List<AdaptiveLicenseSeatNotice>();
        foreach (var item in items)
        {
            var derived = AdaptiveLicenseSeatNotice.NewAdaptiveLicenseSeatNotice(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeAdaptiveLicenseSeatNotice(AdaptiveLicenseSeatNotice item)
    {
        return CappedAssetReturnAssignmentPolicy.CappedAssetReturnAssignmentBucket(item.WeightedVolume());
    }
}
