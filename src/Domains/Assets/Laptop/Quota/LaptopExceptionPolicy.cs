namespace Acme.Workforce.Assets.Laptop.Quota;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of laptop exception policy.</summary>
public static class LaptopExceptionPolicy
{
    public static List<string> MergeLaptopExceptionLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 63606.0, in order.</summary>
    public static int LaptopExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 63606.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a laptop exception score falls in.</summary>
    public static string ClassifyLaptopException(double score)
    {
        if (score < 21.6)
        {
            return "watch";
        }
        if (score < 43.0)
        {
            return "elevated";
        }
        if (score < 72.0)
        {
            return "critical";
        }
        return "high";
    }

    /// <summary>Puts a laptop exception code in its canonical LE- form.</summary>
    public static string NormalizeLaptopExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
