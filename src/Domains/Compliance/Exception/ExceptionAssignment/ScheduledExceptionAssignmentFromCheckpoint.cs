namespace Acme.Workforce.Compliance.Exception.ExceptionAssignment;

using Acme.Workforce.Compliance.Certification.Checkpoint;

/// <summary>Functions of scheduled exception assignment from checkpoint.</summary>
public static class ScheduledExceptionAssignmentFromCheckpoint
{
    public static Dictionary<string, double> SummarizeFallbackCertificationCheckpointsForAssignment(List<FallbackCertificationCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<ScheduledExceptionAssignment> DeriveScheduledExceptionAssignmentsFromCheckpoint(List<FallbackCertificationCheckpoint> items)
    {
        var outValue = new List<ScheduledExceptionAssignment>();
        foreach (var item in items)
        {
            var derived = ScheduledExceptionAssignment.NewScheduledExceptionAssignment(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyScheduledExceptionAssignment(ScheduledExceptionAssignment item)
    {
        return FallbackCertificationCheckpointPolicy.ClassifyFallbackCertificationCheckpoint(item.WeightedExposure());
    }
}
