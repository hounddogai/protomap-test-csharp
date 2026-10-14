namespace Acme.Workforce.Talent.Tests.Nomination.NominationWindow;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Nomination.NominationWindow;

/// <summary>Checks of nomination window code, run by the project's runner.</summary>
public static class NominationSlotEngineChecks
{
    public static void NominationSlotEngineKeepsResultsWithinBounds()
    {
        var engine = NominationSlotEngine.DefaultNominationSlotEngine(Flags.NewRegistry());
        var item = NominationSlot.NewNominationSlot("code-4", "title-697");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        NominationSlotEngineKeepsResultsWithinBounds();
    }
}
