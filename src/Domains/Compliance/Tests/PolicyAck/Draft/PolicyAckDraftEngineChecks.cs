namespace Acme.Workforce.Compliance.Tests.PolicyAck.Draft;

using Acme.Workforce.Compliance.PolicyAck.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PolicyAckDraftEngineChecks
{
    public static void PolicyAckDraftEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyAckDraftEngine.DefaultPolicyAckDraftEngine();
        var item = PolicyAckDraft.NewPolicyAckDraft("code-496", "title-705");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyAckDraftEngineKeepsResultsWithinBounds();
    }
}
