namespace Acme.Workforce.Assets.Assignment.Snapshot;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default assignment snapshot policy.</summary>
public static class DefaultAssignmentSnapshotPolicy
{
    /// <summary>Names the band a default assignment snapshot score falls in.</summary>
    public static string ClassifyDefaultAssignmentSnapshot(double score)
    {
        if (score < 3.1)
        {
            return "high";
        }
        if (score < 53.4)
        {
            return "elevated";
        }
        if (score < 76.4)
        {
            return "ok";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultAssignmentSnapshot()
    {
        var policy = Retry.DefaultPolicy("default-assignment-snapshot");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Counts how many amounts fit in a budget of 9097.0, in order.</summary>
    public static int DefaultAssignmentSnapshotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 9097.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
