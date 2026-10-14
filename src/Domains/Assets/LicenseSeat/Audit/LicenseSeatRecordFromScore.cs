namespace Acme.Workforce.Assets.LicenseSeat.Audit;

using Acme.Workforce.Assets.Assignment.Checkpoint;

/// <summary>Functions of license seat record from score.</summary>
public static class LicenseSeatRecordFromScore
{
    public static Dictionary<string, double> SummarizeAssignmentScoresForRecord(List<AssignmentScore> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<LicenseSeatRecord> DeriveLicenseSeatRecordsFromScore(List<AssignmentScore> items)
    {
        var outValue = new List<LicenseSeatRecord>();
        foreach (var item in items)
        {
            var derived = LicenseSeatRecord.NewLicenseSeatRecord(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileLicenseSeatRecord(LicenseSeatRecord item)
    {
        return StrictAssignmentScorePolicy.NormalizeStrictAssignmentScoreCode(item.Code);
    }
}
