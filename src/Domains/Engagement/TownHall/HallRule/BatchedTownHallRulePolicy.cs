namespace Acme.Workforce.Engagement.TownHall.HallRule;

/// <summary>Functions of batched town hall rule policy.</summary>
public static class BatchedTownHallRulePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 51651.0, in order.</summary>
    public static int BatchedTownHallRuleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 51651.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a batched town hall rule code in its canonical BTHR- form.</summary>
    public static string NormalizeBatchedTownHallRuleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BTHR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BTHR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
