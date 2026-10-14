namespace Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary leave request milestone policy.</summary>
public static class PrimaryLeaveRequestMilestonePolicy
{
    /// <summary>Names the band a primary leave request milestone score falls in.</summary>
    public static string ClassifyPrimaryLeaveRequestMilestone(double score)
    {
        if (score < 40.3)
        {
            return "elevated";
        }
        if (score < 70.5)
        {
            return "low";
        }
        if (score < 75.1)
        {
            return "ok";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 78291.0, in order.</summary>
    public static int PrimaryLeaveRequestMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 78291.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double PrimaryLeaveRequestMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.53;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a primary leave request milestone code in its canonical PLRM- form.</summary>
    public static string NormalizePrimaryLeaveRequestMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PLRM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PLRM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
