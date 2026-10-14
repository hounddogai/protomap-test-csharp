namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of visitor assignment policy.</summary>
public static class VisitorAssignmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeVisitorAssignment()
    {
        var policy = Retry.DefaultPolicy("visitor-assignment");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a visitor assignment score falls in.</summary>
    public static string ClassifyVisitorAssignment(double score)
    {
        if (score < 13.9)
        {
            return "elevated";
        }
        if (score < 48.2)
        {
            return "watch";
        }
        return "medium";
    }
}
