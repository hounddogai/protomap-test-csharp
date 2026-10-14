namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted garnishment budget policy.</summary>
public static class WeightedGarnishmentBudgetPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedGarnishmentBudget()
    {
        var policy = Retry.DefaultPolicy("weighted-garnishment-budget");
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

    /// <summary>Puts a weighted garnishment budget code in its canonical WGB- form.</summary>
    public static string NormalizeWeightedGarnishmentBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WGB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WGB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string WeightedGarnishmentBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static double WeightedGarnishmentBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.89;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
