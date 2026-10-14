namespace Acme.Workforce.Talent.CareerPath.Digest;

/// <summary>Functions of career path proposal policy.</summary>
public static class CareerPathProposalPolicy
{
    /// <summary>Puts a career path proposal code in its canonical CPP- form.</summary>
    public static string NormalizeCareerPathProposalCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CPP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CPP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 28722.0, in order.</summary>
    public static int CareerPathProposalBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 28722.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
