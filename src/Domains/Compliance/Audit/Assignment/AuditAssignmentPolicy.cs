namespace Acme.Workforce.Compliance.Audit.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of audit assignment policy.</summary>
public static class AuditAssignmentPolicy
{
    /// <summary>Names the band a audit assignment score falls in.</summary>
    public static string ClassifyAuditAssignment(double score)
    {
        if (score < 20.3)
        {
            return "high";
        }
        if (score < 27.0)
        {
            return "minimal";
        }
        if (score < 32.9)
        {
            return "medium";
        }
        return "ok";
    }

    public static double AuditAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.73;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 25851.0, in order.</summary>
    public static int AuditAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 25851.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
