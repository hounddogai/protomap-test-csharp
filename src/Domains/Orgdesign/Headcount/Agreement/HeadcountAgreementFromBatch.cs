namespace Acme.Workforce.Orgdesign.Headcount.Agreement;

using Acme.Workforce.Orgdesign.CostModel.Assignment;

/// <summary>Functions of headcount agreement from batch.</summary>
public static class HeadcountAgreementFromBatch
{
    public static Dictionary<string, double> SummarizeCostModelBatchesForAgreement(List<CostModelBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<HeadcountAgreement> DeriveHeadcountAgreementsFromBatch(List<CostModelBatch> items)
    {
        var outValue = new List<HeadcountAgreement>();
        foreach (var item in items)
        {
            var derived = HeadcountAgreement.NewHeadcountAgreement(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreHeadcountAgreement(HeadcountAgreement item)
    {
        return CostModelAssignmentPolicy.ClassifyCostModelAssignment(item.NetExposure());
    }
}
