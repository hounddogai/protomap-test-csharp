namespace Acme.Workforce.Contractors.Vendor.VendorBatch;

using Acme.Workforce.Contractors.Offboarding.Schedule;

/// <summary>Functions of legacy vendor assignment from schedule.</summary>
public static class LegacyVendorAssignmentFromSchedule
{
    public static Dictionary<string, double> SummarizeOffboardingSchedulesForAssignment(List<OffboardingSchedule> items)
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

    public static List<LegacyVendorAssignment> DeriveLegacyVendorAssignmentsFromSchedule(List<OffboardingSchedule> items)
    {
        var outValue = new List<LegacyVendorAssignment>();
        foreach (var item in items)
        {
            var derived = LegacyVendorAssignment.NewLegacyVendorAssignment(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveLegacyVendorAssignment(LegacyVendorAssignment item)
    {
        return OffboardingBaselinePolicy.OffboardingBaselineBucket(item.EffectiveValue());
    }
}
