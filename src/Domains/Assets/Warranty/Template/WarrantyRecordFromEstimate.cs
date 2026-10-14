namespace Acme.Workforce.Assets.Warranty.Template;

using Acme.Workforce.Assets.Assignment.Rule;

/// <summary>Functions of warranty record from estimate.</summary>
public static class WarrantyRecordFromEstimate
{
    public static Dictionary<string, double> SummarizeAssignmentEstimatesForRecord(List<AssignmentEstimate> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<WarrantyRecord> DeriveWarrantyRecordsFromEstimate(List<AssignmentEstimate> items)
    {
        var outValue = new List<WarrantyRecord>();
        foreach (var item in items)
        {
            var derived = WarrantyRecord.NewWarrantyRecord(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceWarrantyRecord(WarrantyRecord item)
    {
        return AssignmentRulePolicy.ClassifyAssignmentRule(item.WeightedVolume());
    }
}
