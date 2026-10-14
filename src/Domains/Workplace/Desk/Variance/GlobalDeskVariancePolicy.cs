namespace Acme.Workforce.Workplace.Desk.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global desk variance policy.</summary>
public static class GlobalDeskVariancePolicy
{
    /// <summary>Puts a global desk variance code in its canonical GDV- form.</summary>
    public static string NormalizeGlobalDeskVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GDV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GDV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string GlobalDeskVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
