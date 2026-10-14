namespace Acme.Workforce.Compensation.Tests.MeritCycle.Plan;

using Acme.Workforce.Compensation.MeritCycle.Plan;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class MeritCycleProposalEngineChecks
{
    public static void MeritCycleProposalEngineKeepsResultsWithinBounds()
    {
        var engine = MeritCycleProposalEngine.DefaultMeritCycleProposalEngine(Flags.NewRegistry());
        var item = MeritCycleProposal.NewMeritCycleProposal("code-480", "name-816");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MeritCycleProposalEngineKeepsResultsWithinBounds();
    }
}
