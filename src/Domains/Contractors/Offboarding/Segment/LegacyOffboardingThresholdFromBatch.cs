namespace Acme.Workforce.Contractors.Offboarding.Segment;

using Acme.Workforce.Contractors.Timesheet.Batch;

/// <summary>Functions of legacy offboarding threshold from batch.</summary>
public static class LegacyOffboardingThresholdFromBatch
{
    public static Dictionary<string, double> SummarizeGlobalTimesheetBatchesForThreshold(List<GlobalTimesheetBatch> items)
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

    public static List<LegacyOffboardingThreshold> DeriveLegacyOffboardingThresholdsFromBatch(List<GlobalTimesheetBatch> items)
    {
        var outValue = new List<LegacyOffboardingThreshold>();
        foreach (var item in items)
        {
            var derived = LegacyOffboardingThreshold.NewLegacyOffboardingThreshold(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeLegacyOffboardingThreshold(LegacyOffboardingThreshold item)
    {
        return CappedTimesheetGroupPolicy.NormalizeCappedTimesheetGroupCode(item.Code);
    }
}
