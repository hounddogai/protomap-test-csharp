namespace Acme.Workforce.Talent.CareerPath.Quota;

using Acme.Workforce.Talent.SuccessionPlan.Allocation;

/// <summary>Functions of career path window from batch.</summary>
public static class CareerPathWindowFromBatch
{
    public static Dictionary<string, double> SummarizeManualSuccessionPlanBatchesForWindow(List<ManualSuccessionPlanBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<CareerPathWindow> DeriveCareerPathWindowsFromBatch(List<ManualSuccessionPlanBatch> items)
    {
        var outValue = new List<CareerPathWindow>();
        foreach (var item in items)
        {
            var derived = CareerPathWindow.NewCareerPathWindow(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateCareerPathWindow(CareerPathWindow item)
    {
        return ManualSuccessionPlanBatchPolicy.ManualSuccessionPlanBatchBucket(item.AdjustedValue());
    }
}
