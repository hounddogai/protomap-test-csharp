namespace Acme.Workforce.Contractors.Tests.Engagement.Policy;

using Acme.Workforce.Contractors.Engagement.Policy;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class LenientEngagementPolicyEngineChecks
{
    public static void LenientEngagementPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = LenientEngagementPolicyEngine.DefaultLenientEngagementPolicyEngine(Flags.NewRegistry());
        var item = LenientEngagementPolicy.NewLenientEngagementPolicy("code-332", "title-759");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientEngagementPolicyEngineKeepsResultsWithinBounds();
    }
}
