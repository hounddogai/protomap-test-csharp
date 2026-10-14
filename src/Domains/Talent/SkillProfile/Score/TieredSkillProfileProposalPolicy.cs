namespace Acme.Workforce.Talent.SkillProfile.Score;

/// <summary>Functions of tiered skill profile proposal policy.</summary>
public static class TieredSkillProfileProposalPolicy
{
    /// <summary>Puts a tiered skill profile proposal code in its canonical TSPP- form.</summary>
    public static string NormalizeTieredSkillProfileProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TSPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TSPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a tiered skill profile proposal score falls in.</summary>
    public static string ClassifyTieredSkillProfileProposal(double score)
    {
        if (score < 5.3)
        {
            return "watch";
        }
        if (score < 75.9)
        {
            return "elevated";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 58681.0, in order.</summary>
    public static int TieredSkillProfileProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 58681.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
