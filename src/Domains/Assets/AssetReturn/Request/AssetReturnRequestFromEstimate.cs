namespace Acme.Workforce.Assets.AssetReturn.Request;

using Acme.Workforce.Assets.Assignment.Estimate;

/// <summary>Functions of asset return request from estimate.</summary>
public static class AssetReturnRequestFromEstimate
{
    public static Dictionary<string, double> SummarizeScheduledAssignmentEstimatesForRequest(List<ScheduledAssignmentEstimate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<AssetReturnRequest> DeriveAssetReturnRequestsFromEstimate(List<ScheduledAssignmentEstimate> items)
    {
        var outValue = new List<AssetReturnRequest>();
        foreach (var item in items)
        {
            var derived = AssetReturnRequest.NewAssetReturnRequest(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyAssetReturnRequest(AssetReturnRequest item)
    {
        return AssignmentMetricPolicy.NormalizeAssignmentMetricCode(item.Code);
    }
}
