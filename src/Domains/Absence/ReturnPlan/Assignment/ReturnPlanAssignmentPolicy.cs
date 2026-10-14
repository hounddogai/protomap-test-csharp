namespace Acme.Workforce.Absence.ReturnPlan.Assignment;

/// <summary>Functions of return plan assignment policy.</summary>
public static class ReturnPlanAssignmentPolicy
{
    /// <summary>Puts a return plan assignment code in its canonical RPA- form.</summary>
    public static string NormalizeReturnPlanAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RPA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RPA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 34770.0, in order.</summary>
    public static int ReturnPlanAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 34770.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
