namespace Acme.Workforce.Orgdesign.Position.Quota;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive position plan policy.</summary>
public static class AdaptivePositionPlanPolicy
{
    public static double AdaptivePositionPlanWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.17;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a adaptive position plan code in its canonical APP- form.</summary>
    public static string NormalizeAdaptivePositionPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("APP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"APP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string AdaptivePositionPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptivePositionPlan()
    {
        var policy = Retry.DefaultPolicy("adaptive-position-plan");
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
}
