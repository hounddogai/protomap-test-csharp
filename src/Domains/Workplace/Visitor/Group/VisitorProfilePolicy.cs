namespace Acme.Workforce.Workplace.Visitor.Group;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of visitor profile policy.</summary>
public static class VisitorProfilePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeVisitorProfile()
    {
        var policy = Retry.DefaultPolicy("visitor-profile");
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

    public static int VisitorProfileSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }
}
