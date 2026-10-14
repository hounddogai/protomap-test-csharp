namespace Acme.Workforce.Talent.Tests.CareerPath.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.CareerPath.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class CareerPathWindowEngineChecks
{
    public static void CareerPathWindowEngineKeepsResultsWithinBounds()
    {
        var engine = CareerPathWindowEngine.DefaultCareerPathWindowEngine(Flags.NewRegistry());
        var item = CareerPathWindow.NewCareerPathWindow("code-195", "title-626");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CareerPathWindowEngineKeepsResultsWithinBounds();
    }
}
