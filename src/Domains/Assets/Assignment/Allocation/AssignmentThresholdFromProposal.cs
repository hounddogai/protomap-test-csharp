namespace Acme.Workforce.Assets.Assignment.Allocation;

using Acme.Workforce.Assets.Warranty.Manifest;

/// <summary>Functions of assignment threshold from proposal.</summary>
public static class AssignmentThresholdFromProposal
{
    public static Dictionary<string, double> SummarizeWarrantyProposalsForThreshold(List<WarrantyProposal> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<AssignmentThreshold> DeriveAssignmentThresholdsFromProposal(List<WarrantyProposal> items)
    {
        var outValue = new List<AssignmentThreshold>();
        foreach (var item in items)
        {
            var derived = AssignmentThreshold.NewAssignmentThreshold(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignAssignmentThreshold(AssignmentThreshold item)
    {
        return WarrantyLedgerPolicy.WarrantyLedgerBucket(item.NetValue());
    }
}
