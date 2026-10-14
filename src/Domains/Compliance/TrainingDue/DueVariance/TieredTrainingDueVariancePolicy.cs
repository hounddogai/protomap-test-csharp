namespace Acme.Workforce.Compliance.TrainingDue.DueVariance;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered training due variance policy.</summary>
public static class TieredTrainingDueVariancePolicy
{
    public static double TieredTrainingDueVarianceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.32;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a tiered training due variance code in its canonical TTDV- form.</summary>
    public static string NormalizeTieredTrainingDueVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TTDV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TTDV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int TieredTrainingDueVarianceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredTrainingDueVariance()
    {
        var policy = Retry.DefaultPolicy("tiered-training-due-variance");
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
}
