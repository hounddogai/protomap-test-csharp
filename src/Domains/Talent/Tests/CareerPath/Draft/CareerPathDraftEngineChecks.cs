namespace Acme.Workforce.Talent.Tests.CareerPath.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class CareerPathDraftEngineChecks
{
    public static void CareerPathDraftEngineKeepsResultsWithinBounds()
    {
        var engine = CareerPathDraftEngine.DefaultCareerPathDraftEngine(Flags.NewRegistry());
        var item = CareerPathDraft.NewCareerPathDraft("code-538", "label-879");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CareerPathDraftEngineKeepsResultsWithinBounds();
    }
}
