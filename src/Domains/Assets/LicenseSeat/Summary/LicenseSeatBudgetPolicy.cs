namespace Acme.Workforce.Assets.LicenseSeat.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of license seat budget policy.</summary>
public static class LicenseSeatBudgetPolicy
{
    public static string LicenseSeatBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a license seat budget code in its canonical LSB- form.</summary>
    public static string NormalizeLicenseSeatBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
