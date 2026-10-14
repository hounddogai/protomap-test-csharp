namespace Acme.Workforce.Assets.Assignment.Summary;

/// <summary>Functions of assignment summary policy.</summary>
public static class AssignmentSummaryPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 48102.0, in order.</summary>
    public static int AssignmentSummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 48102.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a assignment summary code in its canonical AS- form.</summary>
    public static string NormalizeAssignmentSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
