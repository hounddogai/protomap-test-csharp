namespace Acme.Workforce.Compliance.TrainingDue.Baseline;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive training due plan policy.</summary>
public static class AdaptiveTrainingDuePlanPolicy
{
    public static string AdaptiveTrainingDuePlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveTrainingDuePlan()
    {
        var policy = Retry.DefaultPolicy("adaptive-training-due-plan");
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

    public static int AdaptiveTrainingDuePlanSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Puts a adaptive training due plan code in its canonical ATDP- form.</summary>
    public static string NormalizeAdaptiveTrainingDuePlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ATDP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ATDP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
