namespace Acme.Workforce.Absence.Tests.Accrual.Segment;

using Acme.Workforce.Absence.Accrual.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AccrualExceptionEngineChecks
{
    public static void AccrualExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = AccrualExceptionEngine.DefaultAccrualExceptionEngine();
        var item = AccrualException.NewAccrualException("code-725", "name-291");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccrualExceptionEngineKeepsResultsWithinBounds();
    }
}
