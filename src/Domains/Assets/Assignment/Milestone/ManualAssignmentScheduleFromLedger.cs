namespace Acme.Workforce.Assets.Assignment.Milestone;

using Acme.Workforce.Assets.LicenseSeat.Proposal;

/// <summary>Functions of manual assignment schedule from ledger.</summary>
public static class ManualAssignmentScheduleFromLedger
{
    public static Dictionary<string, double> SummarizeLicenseSeatLedgersForSchedule(List<LicenseSeatLedger> items)
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

    public static List<ManualAssignmentSchedule> DeriveManualAssignmentSchedulesFromLedger(List<LicenseSeatLedger> items)
    {
        var outValue = new List<ManualAssignmentSchedule>();
        foreach (var item in items)
        {
            var derived = ManualAssignmentSchedule.NewManualAssignmentSchedule(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchManualAssignmentSchedule(ManualAssignmentSchedule item)
    {
        return LenientLicenseSeatSegmentPolicy.NormalizeLenientLicenseSeatSegmentCode(item.Code);
    }
}
