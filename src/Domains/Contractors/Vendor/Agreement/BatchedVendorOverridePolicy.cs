namespace Acme.Workforce.Contractors.Vendor.Agreement;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of batched vendor override policy.</summary>
public static class BatchedVendorOverridePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeBatchedVendorOverride()
    {
        var policy = Retry.DefaultPolicy("batched-vendor-override");
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

    public static int BatchedVendorOverrideSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Names the band a batched vendor override score falls in.</summary>
    public static string ClassifyBatchedVendorOverride(double score)
    {
        if (score < 26.2)
        {
            return "medium";
        }
        if (score < 28.0)
        {
            return "ok";
        }
        if (score < 53.7)
        {
            return "watch";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 11462.0, in order.</summary>
    public static int BatchedVendorOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 11462.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
