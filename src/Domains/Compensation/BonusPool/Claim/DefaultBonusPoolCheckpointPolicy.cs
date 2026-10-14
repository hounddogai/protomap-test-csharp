namespace Acme.Workforce.Compensation.BonusPool.Claim;

/// <summary>Functions of default bonus pool checkpoint policy.</summary>
public static class DefaultBonusPoolCheckpointPolicy
{
    /// <summary>Puts a default bonus pool checkpoint code in its canonical DBPC- form.</summary>
    public static string NormalizeDefaultBonusPoolCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DBPC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DBPC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 47231.0, in order.</summary>
    public static int DefaultBonusPoolCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 47231.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
