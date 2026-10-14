namespace Acme.Workforce.Orgdesign.CostModel.Score;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted cost model slot policy.</summary>
public static class WeightedCostModelSlotPolicy
{
    /// <summary>Puts a weighted cost model slot code in its canonical WCMS- form.</summary>
    public static string NormalizeWeightedCostModelSlotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WCMS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WCMS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedCostModelSlot()
    {
        var policy = Retry.DefaultPolicy("weighted-cost-model-slot");
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
