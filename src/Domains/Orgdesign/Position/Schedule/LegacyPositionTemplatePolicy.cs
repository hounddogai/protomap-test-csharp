namespace Acme.Workforce.Orgdesign.Position.Schedule;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy position template policy.</summary>
public static class LegacyPositionTemplatePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyPositionTemplate()
    {
        var policy = Retry.DefaultPolicy("legacy-position-template");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int LegacyPositionTemplateSpan(int start, int end)
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
        return span / 60;
    }
}
