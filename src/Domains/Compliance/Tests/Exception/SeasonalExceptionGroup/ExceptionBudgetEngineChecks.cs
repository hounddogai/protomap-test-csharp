namespace Acme.Workforce.Compliance.Tests.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>Checks of seasonal exception group code, run by the project's runner.</summary>
public static class ExceptionBudgetEngineChecks
{
    public static void ExceptionBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionBudgetEngine.DefaultExceptionBudgetEngine();
        var item = ExceptionBudget.NewExceptionBudget("code-807", "name-294");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionBudgetEngineKeepsResultsWithinBounds();
    }
}
