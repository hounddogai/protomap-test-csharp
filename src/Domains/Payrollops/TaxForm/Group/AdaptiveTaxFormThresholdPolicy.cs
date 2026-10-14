namespace Acme.Workforce.Payrollops.TaxForm.Group;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive tax form threshold policy.</summary>
public static class AdaptiveTaxFormThresholdPolicy
{
    public static string AdaptiveTaxFormThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a adaptive tax form threshold code in its canonical ATFT- form.</summary>
    public static string NormalizeAdaptiveTaxFormThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ATFT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ATFT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveTaxFormThreshold()
    {
        var policy = Retry.DefaultPolicy("adaptive-tax-form-threshold");
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
