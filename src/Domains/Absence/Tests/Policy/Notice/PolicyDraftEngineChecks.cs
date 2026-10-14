namespace Acme.Workforce.Absence.Tests.Policy.Notice;

using Acme.Workforce.Absence.Policy.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PolicyDraftEngineChecks
{
    public static void PolicyDraftEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyDraftEngine.DefaultPolicyDraftEngine(Flags.NewRegistry());
        var item = PolicyDraft.NewPolicyDraft("code-367", "label-836");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyDraftEngineKeepsResultsWithinBounds();
    }
}
