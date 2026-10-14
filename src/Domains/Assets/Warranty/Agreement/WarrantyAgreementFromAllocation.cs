namespace Acme.Workforce.Assets.Warranty.Agreement;

using Acme.Workforce.Assets.Assignment.Allocation;

/// <summary>Functions of warranty agreement from allocation.</summary>
public static class WarrantyAgreementFromAllocation
{
    public static Dictionary<string, double> SummarizeAssignmentAllocationsForAgreement(List<AssignmentAllocation> items)
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

    public static List<WarrantyAgreement> DeriveWarrantyAgreementsFromAllocation(List<AssignmentAllocation> items)
    {
        var outValue = new List<WarrantyAgreement>();
        foreach (var item in items)
        {
            var derived = WarrantyAgreement.NewWarrantyAgreement(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanWarrantyAgreement(WarrantyAgreement item)
    {
        return AssignmentThresholdPolicy.AssignmentThresholdWeight(1, true);
    }
}
