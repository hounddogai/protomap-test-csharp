namespace Acme.Workforce.Workplace.Visitor.Score;

/// <summary>Functions of seasonal visitor checkpoint policy.</summary>
public static class SeasonalVisitorCheckpointPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 54584.0, in order.</summary>
    public static int SeasonalVisitorCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 54584.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a seasonal visitor checkpoint code in its canonical SVC- form.</summary>
    public static string NormalizeSeasonalVisitorCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SVC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SVC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
