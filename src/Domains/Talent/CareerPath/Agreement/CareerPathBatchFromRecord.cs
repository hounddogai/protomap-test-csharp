namespace Acme.Workforce.Talent.CareerPath.Agreement;

using Acme.Workforce.Talent.SuccessionPlan.Ledger;

/// <summary>Functions of career path batch from record.</summary>
public static class CareerPathBatchFromRecord
{
    public static Dictionary<string, double> SummarizeSuccessionPlanRecordsForBatch(List<SuccessionPlanRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<CareerPathBatch> DeriveCareerPathBatchesFromRecord(List<SuccessionPlanRecord> items)
    {
        var outValue = new List<CareerPathBatch>();
        foreach (var item in items)
        {
            var derived = CareerPathBatch.NewCareerPathBatch(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitCareerPathBatch(CareerPathBatch item)
    {
        return SuccessionPlanManifestPolicy.SuccessionPlanManifestBucket(item.MeasuredValue());
    }
}
