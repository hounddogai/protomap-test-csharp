namespace Acme.Workforce.Talent.Tests.Nomination.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Nomination.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class NominationScheduleEngineChecks
{
    public static void NominationScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = NominationScheduleEngine.DefaultNominationScheduleEngine(Flags.NewRegistry());
        var item = NominationSchedule.NewNominationSchedule("code-324", "name-266");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        NominationScheduleEngineKeepsResultsWithinBounds();
    }
}
