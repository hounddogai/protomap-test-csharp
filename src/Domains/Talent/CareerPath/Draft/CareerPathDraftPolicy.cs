namespace Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>Functions of career path draft policy.</summary>
public static class CareerPathDraftPolicy
{
    /// <summary>Puts a career path draft code in its canonical CPD- form.</summary>
    public static string NormalizeCareerPathDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CPD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CPD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 68302.0, in order.</summary>
    public static int CareerPathDraftBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 68302.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
