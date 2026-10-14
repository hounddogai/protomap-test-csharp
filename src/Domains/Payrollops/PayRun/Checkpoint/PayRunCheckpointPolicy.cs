namespace Acme.Workforce.Payrollops.PayRun.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pay run checkpoint policy.</summary>
public static class PayRunCheckpointPolicy
{
    public static string PayRunCheckpointBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a pay run checkpoint code in its canonical PRC- form.</summary>
    public static string NormalizePayRunCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
