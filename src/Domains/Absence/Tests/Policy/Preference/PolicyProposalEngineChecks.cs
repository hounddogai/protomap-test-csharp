namespace Acme.Workforce.Absence.Tests.Policy.Preference;

using Acme.Workforce.Absence.Policy.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PolicyProposalEngineChecks
{
    public static void PolicyProposalEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyProposalEngine.DefaultPolicyProposalEngine();
        var item = PolicyProposal.NewPolicyProposal("code-626", "name-613");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyProposalEngineKeepsResultsWithinBounds();
    }
}
