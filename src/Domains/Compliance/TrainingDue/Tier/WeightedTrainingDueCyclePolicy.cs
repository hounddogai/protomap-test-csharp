namespace Acme.Workforce.Compliance.TrainingDue.Tier;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted training due cycle policy.</summary>
public static class WeightedTrainingDueCyclePolicy
{
    public static string WeightedTrainingDueCycleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int WeightedTrainingDueCycleSpan(int start, int end)
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

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedTrainingDueCycle()
    {
        var policy = Retry.DefaultPolicy("weighted-training-due-cycle");
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

    /// <summary>Puts a weighted training due cycle code in its canonical WTDC- form.</summary>
    public static string NormalizeWeightedTrainingDueCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WTDC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WTDC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
