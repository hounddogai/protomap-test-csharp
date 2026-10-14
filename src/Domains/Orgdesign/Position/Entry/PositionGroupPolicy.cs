namespace Acme.Workforce.Orgdesign.Position.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of position group policy.</summary>
public static class PositionGroupPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 97207.0, in order.</summary>
    public static int PositionGroupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 97207.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a position group score falls in.</summary>
    public static string ClassifyPositionGroup(double score)
    {
        if (score < 7.5)
        {
            return "low";
        }
        if (score < 14.6)
        {
            return "elevated";
        }
        if (score < 22.9)
        {
            return "ok";
        }
        if (score < 31.7)
        {
            return "high";
        }
        return "minimal";
    }

    /// <summary>Puts a position group code in its canonical PG- form.</summary>
    public static string NormalizePositionGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PositionGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.75;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
