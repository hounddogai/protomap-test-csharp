namespace Acme.Workforce.Compliance.Exception.Rule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched exception record policy.</summary>
public static class BatchedExceptionRecordPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 21715.0, in order.</summary>
    public static int BatchedExceptionRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 21715.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double BatchedExceptionRecordWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.7;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a batched exception record score falls in.</summary>
    public static string ClassifyBatchedExceptionRecord(double score)
    {
        if (score < 71.7)
        {
            return "watch";
        }
        if (score < 84.8)
        {
            return "medium";
        }
        if (score < 85.0)
        {
            return "elevated";
        }
        if (score < 98.5)
        {
            return "minimal";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedExceptionRecord()
    {
        var policy = Retry.DefaultPolicy("batched-exception-record");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
