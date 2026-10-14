namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Bundle;

using Acme.Workforce.Contractors.StatementOfWork.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class StatementOfWorkBundleEngineChecks
{
    public static void StatementOfWorkBundleEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkBundleEngine.DefaultStatementOfWorkBundleEngine();
        var item = StatementOfWorkBundle.NewStatementOfWorkBundle("code-551", "name-155");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkBundleEngineKeepsResultsWithinBounds();
    }
}
