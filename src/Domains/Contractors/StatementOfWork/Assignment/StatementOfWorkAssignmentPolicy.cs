namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of statement of work assignment policy.</summary>
public static class StatementOfWorkAssignmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStatementOfWorkAssignment()
    {
        var policy = Retry.DefaultPolicy("statement-of-work-assignment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a statement of work assignment score falls in.</summary>
    public static string ClassifyStatementOfWorkAssignment(double score)
    {
        if (score < 14.8)
        {
            return "elevated";
        }
        if (score < 42.1)
        {
            return "critical";
        }
        if (score < 72.2)
        {
            return "watch";
        }
        if (score < 92.7)
        {
            return "medium";
        }
        return "minimal";
    }

    public static double StatementOfWorkAssignmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.72;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
