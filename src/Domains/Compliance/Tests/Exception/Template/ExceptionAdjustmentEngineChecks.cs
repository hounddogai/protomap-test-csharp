namespace Acme.Workforce.Compliance.Tests.Exception.Template;

using Acme.Workforce.Compliance.Exception.Template;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ExceptionAdjustmentEngineChecks
{
    public static void ExceptionAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionAdjustmentEngine.DefaultExceptionAdjustmentEngine(Flags.NewRegistry());
        var item = ExceptionAdjustment.NewExceptionAdjustment("code-459", "title-922");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionAdjustmentEngineKeepsResultsWithinBounds();
    }
}
