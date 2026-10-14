namespace Acme.Workforce.Talent.Tests.CareerPath.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.CareerPath.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class CareerPathExceptionEngineChecks
{
    public static void CareerPathExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = CareerPathExceptionEngine.DefaultCareerPathExceptionEngine(Flags.NewRegistry());
        var item = CareerPathException.NewCareerPathException("code-782", "title-483");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CareerPathExceptionEngineKeepsResultsWithinBounds();
    }
}
