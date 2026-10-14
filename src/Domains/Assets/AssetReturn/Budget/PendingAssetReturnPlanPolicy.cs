namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending asset return plan policy.</summary>
public static class PendingAssetReturnPlanPolicy
{
    public static string PendingAssetReturnPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingAssetReturnPlan()
    {
        var policy = Retry.DefaultPolicy("pending-asset-return-plan");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a pending asset return plan score falls in.</summary>
    public static string ClassifyPendingAssetReturnPlan(double score)
    {
        if (score < 13.3)
        {
            return "critical";
        }
        if (score < 40.2)
        {
            return "watch";
        }
        if (score < 97.5)
        {
            return "ok";
        }
        return "high";
    }

    public static int PendingAssetReturnPlanSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }
}
