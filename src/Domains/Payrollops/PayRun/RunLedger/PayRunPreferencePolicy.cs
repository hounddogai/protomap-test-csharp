namespace Acme.Workforce.Payrollops.PayRun.RunLedger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pay run preference policy.</summary>
public static class PayRunPreferencePolicy
{
    /// <summary>Puts a pay run preference code in its canonical PRP- form.</summary>
    public static string NormalizePayRunPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PayRunPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.08;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
