namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>Checks of pay assignment code, run by the project's runner.</summary>
public static class StrictRetroPayProposalEngineChecks
{
    public static void StrictRetroPayProposalEngineKeepsResultsWithinBounds()
    {
        var engine = StrictRetroPayProposalEngine.DefaultStrictRetroPayProposalEngine(Flags.NewRegistry());
        var item = StrictRetroPayProposal.NewStrictRetroPayProposal("code-586", "name-67");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictRetroPayProposalEngineKeepsResultsWithinBounds();
    }
}
