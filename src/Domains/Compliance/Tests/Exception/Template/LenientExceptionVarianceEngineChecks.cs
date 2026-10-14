namespace Acme.Workforce.Compliance.Tests.Exception.Template;

using Acme.Workforce.Compliance.Exception.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LenientExceptionVarianceEngineChecks
{
    public static void LenientExceptionVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = LenientExceptionVarianceEngine.DefaultLenientExceptionVarianceEngine();
        var item = LenientExceptionVariance.NewLenientExceptionVariance("code-37", "label-18");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientExceptionVarianceEngineKeepsResultsWithinBounds();
    }
}
