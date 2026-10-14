namespace Acme.Workforce.Compliance.Tests.PolicyAck.Notice;

using Acme.Workforce.Compliance.PolicyAck.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PolicyAckNoticeEngineChecks
{
    public static void PolicyAckNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyAckNoticeEngine.DefaultPolicyAckNoticeEngine();
        var item = PolicyAckNotice.NewPolicyAckNotice("code-525", "title-596");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyAckNoticeEngineKeepsResultsWithinBounds();
    }
}
