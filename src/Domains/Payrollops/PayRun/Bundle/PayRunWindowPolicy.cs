namespace Acme.Workforce.Payrollops.PayRun.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pay run window policy.</summary>
public static class PayRunWindowPolicy
{
    /// <summary>Puts a pay run window code in its canonical PRW- form.</summary>
    public static string NormalizePayRunWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string PayRunWindowBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
