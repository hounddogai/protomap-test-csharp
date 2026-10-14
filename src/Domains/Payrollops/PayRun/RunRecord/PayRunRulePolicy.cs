namespace Acme.Workforce.Payrollops.PayRun.RunRecord;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pay run rule policy.</summary>
public static class PayRunRulePolicy
{
    public static string PayRunRuleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a pay run rule code in its canonical PRR- form.</summary>
    public static string NormalizePayRunRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
