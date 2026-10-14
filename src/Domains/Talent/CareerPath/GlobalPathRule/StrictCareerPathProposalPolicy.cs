namespace Acme.Workforce.Talent.CareerPath.GlobalPathRule;

/// <summary>Functions of strict career path proposal policy.</summary>
public static class StrictCareerPathProposalPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 58355.0, in order.</summary>
    public static int StrictCareerPathProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 58355.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a strict career path proposal code in its canonical SCPP- form.</summary>
    public static string NormalizeStrictCareerPathProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SCPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SCPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
