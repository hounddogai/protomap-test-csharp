namespace Acme.Workforce.Assets.Laptop.Audit;

/// <summary>Functions of rolling laptop exception policy.</summary>
public static class RollingLaptopExceptionPolicy
{
    /// <summary>Names the band a rolling laptop exception score falls in.</summary>
    public static string ClassifyRollingLaptopException(double score)
    {
        if (score < 20.2)
        {
            return "low";
        }
        if (score < 49.6)
        {
            return "minimal";
        }
        if (score < 81.4)
        {
            return "medium";
        }
        if (score < 86.3)
        {
            return "critical";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 58044.0, in order.</summary>
    public static int RollingLaptopExceptionBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 58044.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a rolling laptop exception code in its canonical RLE- form.</summary>
    public static string NormalizeRollingLaptopExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RLE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RLE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
