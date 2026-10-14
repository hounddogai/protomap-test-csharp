namespace Acme.Workforce.Payrollops.Garnishment.Target;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted garnishment request policy.</summary>
public static class WeightedGarnishmentRequestPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedGarnishmentRequest()
    {
        var policy = Retry.DefaultPolicy("weighted-garnishment-request");
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

    /// <summary>Puts a weighted garnishment request code in its canonical WGR- form.</summary>
    public static string NormalizeWeightedGarnishmentRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WGR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WGR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
