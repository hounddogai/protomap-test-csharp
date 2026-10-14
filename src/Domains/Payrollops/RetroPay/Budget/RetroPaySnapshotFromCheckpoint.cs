namespace Acme.Workforce.Payrollops.RetroPay.Budget;

using Acme.Workforce.Payrollops.PayRun.Checkpoint;
using Acme.Workforce.Payrollops.RetroPay.Checkpoint;

/// <summary>Functions of retro pay snapshot from checkpoint.</summary>
public static class RetroPaySnapshotFromCheckpoint
{
    public static Dictionary<string, double> SummarizePayRunCheckpointsForSnapshot(List<PayRunCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<RetroPaySnapshot> DeriveRetroPaySnapshotsFromCheckpoint(List<PayRunCheckpoint> items)
    {
        var outValue = new List<RetroPaySnapshot>();
        foreach (var item in items)
        {
            var derived = RetroPaySnapshot.NewRetroPaySnapshot(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeRetroPaySnapshot(RetroPaySnapshot item)
    {
        return PayRunCheckpointPolicy.NormalizePayRunCheckpointCode(item.Code);
    }

    public static Dictionary<string, double> SummarizeRetroPayCheckpointsForSnapshot(List<RetroPayCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<RetroPaySnapshot> DeriveRetroPaySnapshotsFromCheckpointExtra(List<RetroPayCheckpoint> items)
    {
        var outValue = new List<RetroPaySnapshot>();
        foreach (var item in items)
        {
            var derived = RetroPaySnapshot.NewRetroPaySnapshot(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ClassifyRetroPaySnapshot(RetroPaySnapshot item)
    {
        return RetroPayPreferencePolicy.RetroPayPreferenceWeight(9, true);
    }
}
