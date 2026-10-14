namespace Acme.Workforce.Talent.CareerPath.Ledger;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of career path group policy.</summary>
public static class CareerPathGroupPolicy
{
    public static int CareerPathGroupSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCareerPathGroup()
    {
        var policy = Retry.DefaultPolicy("career-path-group");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
