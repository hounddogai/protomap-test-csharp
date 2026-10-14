namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary visitor assignment policy.</summary>
public static class PrimaryVisitorAssignmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 50115.0, in order.</summary>
    public static int PrimaryVisitorAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 50115.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a primary visitor assignment code in its canonical PVA- form.</summary>
    public static string NormalizePrimaryVisitorAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PVA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PVA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double PrimaryVisitorAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.83;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
