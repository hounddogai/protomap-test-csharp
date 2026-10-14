namespace Acme.Workforce.Contractors.StatementOfWork.Digest;

using Acme.Workforce.Contractors.StatementOfWork.Assignment;

/// <summary>Functions of statement of work profile from checkpoint.</summary>
public static class StatementOfWorkProfileFromCheckpoint
{
    public static Dictionary<string, double> SummarizeStatementOfWorkCheckpointsForProfile(List<StatementOfWorkCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<StatementOfWorkProfile> DeriveStatementOfWorkProfilesFromCheckpoint(List<StatementOfWorkCheckpoint> items)
    {
        var outValue = new List<StatementOfWorkProfile>();
        foreach (var item in items)
        {
            var derived = StatementOfWorkProfile.NewStatementOfWorkProfile(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignStatementOfWorkProfile(StatementOfWorkProfile item)
    {
        return StatementOfWorkSchedulePolicy.StatementOfWorkScheduleBucket(item.NetLoad());
    }
}
