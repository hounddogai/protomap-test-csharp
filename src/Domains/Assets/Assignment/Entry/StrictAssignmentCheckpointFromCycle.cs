namespace Acme.Workforce.Assets.Assignment.Entry;

using Acme.Workforce.Assets.LicenseSeat.Cycle;

/// <summary>Functions of strict assignment checkpoint from cycle.</summary>
public static class StrictAssignmentCheckpointFromCycle
{
    public static Dictionary<string, double> SummarizeCappedLicenseSeatCyclesForCheckpoint(List<CappedLicenseSeatCycle> items)
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

    public static List<StrictAssignmentCheckpoint> DeriveStrictAssignmentCheckpointsFromCycle(List<CappedLicenseSeatCycle> items)
    {
        var outValue = new List<StrictAssignmentCheckpoint>();
        foreach (var item in items)
        {
            var derived = StrictAssignmentCheckpoint.NewStrictAssignmentCheckpoint(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectStrictAssignmentCheckpoint(StrictAssignmentCheckpoint item)
    {
        return LenientLicenseSeatAuditPolicy.LenientLicenseSeatAuditWeight(7, true);
    }
}
