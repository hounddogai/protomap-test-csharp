namespace Acme.Workforce.Orgdesign.CostModel.Policy;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of cost model exception policy.</summary>
public static class CostModelExceptionPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCostModelException()
    {
        var policy = Retry.DefaultPolicy("cost-model-exception");
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

    /// <summary>Puts a cost model exception code in its canonical CME- form.</summary>
    public static string NormalizeCostModelExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CME-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CME-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
