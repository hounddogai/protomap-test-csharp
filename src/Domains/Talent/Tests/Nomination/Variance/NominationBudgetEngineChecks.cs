namespace Acme.Workforce.Talent.Tests.Nomination.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Nomination.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class NominationBudgetEngineChecks
{
    public static void NominationBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = NominationBudgetEngine.DefaultNominationBudgetEngine(Flags.NewRegistry());
        var item = NominationBudget.NewNominationBudget("code-557", "title-93");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        NominationBudgetEngineKeepsResultsWithinBounds();
    }
}
