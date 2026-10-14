namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using Acme.Workforce.Contractors.Offboarding.Mapping;

/// <summary>Functions of offboarding assignment from record.</summary>
public static class OffboardingAssignmentFromRecord
{
    public static Dictionary<string, double> SummarizeOffboardingRecordsForAssignment(List<OffboardingRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<OffboardingAssignment> DeriveOffboardingAssignmentsFromRecord(List<OffboardingRecord> items)
    {
        var outValue = new List<OffboardingAssignment>();
        foreach (var item in items)
        {
            var derived = OffboardingAssignment.NewOffboardingAssignment(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateOffboardingAssignment(OffboardingAssignment item)
    {
        return WeightedOffboardingThresholdPolicy.WeightedOffboardingThresholdBucket(item.MeasuredValue());
    }
}
