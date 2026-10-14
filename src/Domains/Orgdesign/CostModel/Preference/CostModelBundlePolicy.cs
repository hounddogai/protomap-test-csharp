namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of cost model bundle policy.</summary>
public static class CostModelBundlePolicy
{
    /// <summary>Names the band a cost model bundle score falls in.</summary>
    public static string ClassifyCostModelBundle(double score)
    {
        if (score < 3.3)
        {
            return "medium";
        }
        if (score < 69.6)
        {
            return "ok";
        }
        if (score < 84.5)
        {
            return "low";
        }
        return "elevated";
    }

    /// <summary>Puts a cost model bundle code in its canonical CMB- form.</summary>
    public static string NormalizeCostModelBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CMB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CMB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string CostModelBundleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCostModelBundle()
    {
        var policy = Retry.DefaultPolicy("cost-model-bundle");
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
