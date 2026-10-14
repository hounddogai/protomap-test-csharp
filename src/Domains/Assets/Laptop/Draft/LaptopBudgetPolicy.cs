namespace Acme.Workforce.Assets.Laptop.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of laptop budget policy.</summary>
public static class LaptopBudgetPolicy
{
    public static double LaptopBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.71;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a laptop budget code in its canonical LB- form.</summary>
    public static string NormalizeLaptopBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
