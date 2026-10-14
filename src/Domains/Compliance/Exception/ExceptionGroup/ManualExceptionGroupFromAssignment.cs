namespace Acme.Workforce.Compliance.Exception.ExceptionGroup;

using Acme.Workforce.Compliance.Exception.Rule;

/// <summary>Functions of manual exception group from assignment.</summary>
public static class ManualExceptionGroupFromAssignment
{
    public static Dictionary<string, double> SummarizeFallbackExceptionAssignmentsForGroup(List<FallbackExceptionAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<ManualExceptionGroup> DeriveManualExceptionGroupsFromAssignment(List<FallbackExceptionAssignment> items)
    {
        var outValue = new List<ManualExceptionGroup>();
        foreach (var item in items)
        {
            var derived = ManualExceptionGroup.NewManualExceptionGroup(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankManualExceptionGroup(ManualExceptionGroup item)
    {
        return BatchedExceptionRecordPolicy.BatchedExceptionRecordWeight(1, true);
    }
}
