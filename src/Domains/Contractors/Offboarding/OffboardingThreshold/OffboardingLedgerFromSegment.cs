namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

using Acme.Workforce.Contractors.Timesheet.Policy;

/// <summary>Functions of offboarding ledger from segment.</summary>
public static class OffboardingLedgerFromSegment
{
    public static Dictionary<string, double> SummarizeTimesheetSegmentsForLedger(List<TimesheetSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<OffboardingLedger> DeriveOffboardingLedgersFromSegment(List<TimesheetSegment> items)
    {
        var outValue = new List<OffboardingLedger>();
        foreach (var item in items)
        {
            var derived = OffboardingLedger.NewOffboardingLedger(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchOffboardingLedger(OffboardingLedger item)
    {
        return TimesheetPolicyPolicy.NormalizeTimesheetPolicyCode(item.Code);
    }
}
