namespace Acme.Workforce.Absence.Accrual.Checkpoint;

using Acme.Workforce.Absence.Accrual.Notice;

/// <summary>Functions of manual accrual checkpoint from notice.</summary>
public static class ManualAccrualCheckpointFromNotice
{
    public static Dictionary<string, double> SummarizePrimaryAccrualNoticesForCheckpoint(List<PrimaryAccrualNotice> items)
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

    public static List<ManualAccrualCheckpoint> DeriveManualAccrualCheckpointsFromNotice(List<PrimaryAccrualNotice> items)
    {
        var outValue = new List<ManualAccrualCheckpoint>();
        foreach (var item in items)
        {
            var derived = ManualAccrualCheckpoint.NewManualAccrualCheckpoint(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankManualAccrualCheckpoint(ManualAccrualCheckpoint item)
    {
        return BatchedAccrualCheckpointPolicy.ClassifyBatchedAccrualCheckpoint(item.EffectiveExposure());
    }
}
