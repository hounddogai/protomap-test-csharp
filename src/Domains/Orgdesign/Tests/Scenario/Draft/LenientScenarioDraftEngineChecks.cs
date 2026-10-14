namespace Acme.Workforce.Orgdesign.Tests.Scenario.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Scenario.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientScenarioDraftEngineChecks
{
    public static void LenientScenarioDraftEngineKeepsResultsWithinBounds()
    {
        var engine = LenientScenarioDraftEngine.DefaultLenientScenarioDraftEngine(Flags.NewRegistry());
        var item = LenientScenarioDraft.NewLenientScenarioDraft("code-926", "name-762");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientScenarioDraftEngineKeepsResultsWithinBounds();
    }
}
