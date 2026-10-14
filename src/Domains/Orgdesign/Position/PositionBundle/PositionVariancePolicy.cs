namespace Acme.Workforce.Orgdesign.Position.PositionBundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of position variance policy.</summary>
public static class PositionVariancePolicy
{
    /// <summary>Names the band a position variance score falls in.</summary>
    public static string ClassifyPositionVariance(double score)
    {
        if (score < 17.4)
        {
            return "ok";
        }
        if (score < 18.3)
        {
            return "critical";
        }
        if (score < 24.4)
        {
            return "high";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 83133.0, in order.</summary>
    public static int PositionVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 83133.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a position variance code in its canonical PV- form.</summary>
    public static string NormalizePositionVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string PositionVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
