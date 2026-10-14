namespace Acme.Workforce.Compliance.Exception.ExceptionAssignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of scheduled exception assignment policy.</summary>
public static class ScheduledExceptionAssignmentPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 86367.0, in order.</summary>
    public static int ScheduledExceptionAssignmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 86367.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a scheduled exception assignment score falls in.</summary>
    public static string ClassifyScheduledExceptionAssignment(double score)
    {
        if (score < 38.3)
        {
            return "low";
        }
        if (score < 39.4)
        {
            return "critical";
        }
        return "watch";
    }

    public static int ScheduledExceptionAssignmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    public static double ScheduledExceptionAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.38;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
