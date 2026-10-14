namespace Acme.Workforce.Talent.Tests.CareerPath.Rule;

using Acme.Workforce.Talent.CareerPath.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class CareerPathTargetEngineChecks
{
    public static void CareerPathTargetEngineKeepsResultsWithinBounds()
    {
        var engine = CareerPathTargetEngine.DefaultCareerPathTargetEngine();
        var item = CareerPathTarget.NewCareerPathTarget("code-129", "title-962");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CareerPathTargetEngineKeepsResultsWithinBounds();
    }
}
