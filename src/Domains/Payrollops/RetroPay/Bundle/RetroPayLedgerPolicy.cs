namespace Acme.Workforce.Payrollops.RetroPay.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of retro pay ledger policy.</summary>
public static class RetroPayLedgerPolicy
{
    public static int RetroPayLedgerSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a retro pay ledger code in its canonical RPL- form.</summary>
    public static string NormalizeRetroPayLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string RetroPayLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double RetroPayLedgerWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.14;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
