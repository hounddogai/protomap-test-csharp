namespace Acme.Workforce.Assets.Assignment.Request;

using Acme.Workforce.Assets.Assignment.Batch;

/// <summary>Functions of assignment request from tier.</summary>
public static class AssignmentRequestFromTier
{
    public static Dictionary<string, double> SummarizePrimaryAssignmentTiersForRequest(List<PrimaryAssignmentTier> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<AssignmentRequest> DeriveAssignmentRequestsFromTier(List<PrimaryAssignmentTier> items)
    {
        var outValue = new List<AssignmentRequest>();
        foreach (var item in items)
        {
            var derived = AssignmentRequest.NewAssignmentRequest(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectAssignmentRequest(AssignmentRequest item)
    {
        return PrimaryAssignmentTierPolicy.PrimaryAssignmentTierBucket(item.MeasuredValue());
    }
}
