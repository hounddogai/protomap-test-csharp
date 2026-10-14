namespace Acme.Workforce.Contractors.StatementOfWork.Ledger;

using Acme.Workforce.Contractors.Offboarding.Cycle;

/// <summary>Functions of regional statement of work digest from cycle.</summary>
public static class RegionalStatementOfWorkDigestFromCycle
{
    public static Dictionary<string, double> SummarizeOffboardingCyclesForDigest(List<OffboardingCycle> items)
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

    public static List<RegionalStatementOfWorkDigest> DeriveRegionalStatementOfWorkDigestsFromCycle(List<OffboardingCycle> items)
    {
        var outValue = new List<RegionalStatementOfWorkDigest>();
        foreach (var item in items)
        {
            var derived = RegionalStatementOfWorkDigest.NewRegionalStatementOfWorkDigest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectRegionalStatementOfWorkDigest(RegionalStatementOfWorkDigest item)
    {
        return OffboardingCyclePolicy.ClassifyOffboardingCycle(item.ProjectedVolume());
    }
}
