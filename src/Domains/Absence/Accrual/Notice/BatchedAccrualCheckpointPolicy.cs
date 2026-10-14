namespace Acme.Workforce.Absence.Accrual.Notice;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of batched accrual checkpoint policy.</summary>
public static class BatchedAccrualCheckpointPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedAccrualCheckpoint()
    {
        var policy = Retry.DefaultPolicy("batched-accrual-checkpoint");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static double BatchedAccrualCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.07;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a batched accrual checkpoint score falls in.</summary>
    public static string ClassifyBatchedAccrualCheckpoint(double score)
    {
        if (score < 21.1)
        {
            return "low";
        }
        if (score < 57.2)
        {
            return "watch";
        }
        if (score < 93.4)
        {
            return "elevated";
        }
        return "critical";
    }

    public static List<string> MergeBatchedAccrualCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
