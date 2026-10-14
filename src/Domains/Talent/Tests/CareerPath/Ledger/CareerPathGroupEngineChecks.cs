namespace Acme.Workforce.Talent.Tests.CareerPath.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CareerPathGroupEngineChecks
{
    public static void CareerPathGroupEngineKeepsResultsWithinBounds()
    {
        var engine = CareerPathGroupEngine.DefaultCareerPathGroupEngine(Flags.NewRegistry());
        var item = CareerPathGroup.NewCareerPathGroup("code-234", "label-921");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CareerPathGroupEngineKeepsResultsWithinBounds();
    }
}
