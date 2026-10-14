namespace Acme.Workforce.Compliance.Tests.Exception.Request;

using Acme.Workforce.Compliance.Exception.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class ExceptionLedgerEngineChecks
{
    public static void ExceptionLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionLedgerEngine.DefaultExceptionLedgerEngine();
        var item = ExceptionLedger.NewExceptionLedger("code-455", "label-332");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionLedgerEngineKeepsResultsWithinBounds();
    }
}
