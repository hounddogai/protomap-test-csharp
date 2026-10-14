namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted adjustment draft policy.</summary>
public static class WeightedAdjustmentDraftPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedAdjustmentDraft()
    {
        var policy = Retry.DefaultPolicy("weighted-adjustment-draft");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a weighted adjustment draft code in its canonical WAD- form.</summary>
    public static string NormalizeWeightedAdjustmentDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WAD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WAD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a weighted adjustment draft score falls in.</summary>
    public static string ClassifyWeightedAdjustmentDraft(double score)
    {
        if (score < 41.5)
        {
            return "elevated";
        }
        if (score < 81.4)
        {
            return "ok";
        }
        return "watch";
    }
}
