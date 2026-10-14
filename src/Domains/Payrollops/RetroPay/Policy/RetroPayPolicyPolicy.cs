namespace Acme.Workforce.Payrollops.RetroPay.Policy;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of retro pay policy policy.</summary>
public static class RetroPayPolicyPolicy
{
    /// <summary>Names the band a retro pay policy score falls in.</summary>
    public static string ClassifyRetroPayPolicy(double score)
    {
        if (score < 4.7)
        {
            return "high";
        }
        if (score < 48.4)
        {
            return "ok";
        }
        if (score < 85.7)
        {
            return "elevated";
        }
        return "low";
    }

    /// <summary>Puts a retro pay policy code in its canonical RPP- form.</summary>
    public static string NormalizeRetroPayPolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRetroPayPolicy()
    {
        var policy = Retry.DefaultPolicy("retro-pay-policy");
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
}
