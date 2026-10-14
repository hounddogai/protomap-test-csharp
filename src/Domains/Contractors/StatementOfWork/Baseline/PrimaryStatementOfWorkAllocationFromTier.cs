namespace Acme.Workforce.Contractors.StatementOfWork.Baseline;

using Acme.Workforce.Contractors.StatementOfWork.Tier;

/// <summary>Functions of primary statement of work allocation from tier.</summary>
public static class PrimaryStatementOfWorkAllocationFromTier
{
    public static Dictionary<string, double> SummarizeStatementOfWorkTiersForAllocation(List<StatementOfWorkTier> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<PrimaryStatementOfWorkAllocation> DerivePrimaryStatementOfWorkAllocationsFromTier(List<StatementOfWorkTier> items)
    {
        var outValue = new List<PrimaryStatementOfWorkAllocation>();
        foreach (var item in items)
        {
            var derived = PrimaryStatementOfWorkAllocation.NewPrimaryStatementOfWorkAllocation(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankPrimaryStatementOfWorkAllocation(PrimaryStatementOfWorkAllocation item)
    {
        return LenientStatementOfWorkBundlePolicy.NormalizeLenientStatementOfWorkBundleCode(item.Code);
    }
}
