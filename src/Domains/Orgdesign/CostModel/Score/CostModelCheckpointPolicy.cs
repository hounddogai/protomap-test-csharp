namespace Acme.Workforce.Orgdesign.CostModel.Score;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of cost model checkpoint policy.</summary>
public static class CostModelCheckpointPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCostModelCheckpoint()
    {
        var policy = Retry.DefaultPolicy("cost-model-checkpoint");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a cost model checkpoint code in its canonical CMC- form.</summary>
    public static string NormalizeCostModelCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CMC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CMC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
