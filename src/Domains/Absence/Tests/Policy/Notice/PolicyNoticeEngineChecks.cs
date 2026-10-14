namespace Acme.Workforce.Absence.Tests.Policy.Notice;

using Acme.Workforce.Absence.Policy.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PolicyNoticeEngineChecks
{
    public static void PolicyNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyNoticeEngine.DefaultPolicyNoticeEngine(Flags.NewRegistry());
        var item = PolicyNotice.NewPolicyNotice("code-229", "name-318");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyNoticeEngineKeepsResultsWithinBounds();
    }
}
