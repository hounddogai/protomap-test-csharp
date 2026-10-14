namespace Acme.Workforce.Absence.Tests.Balance.RecordItem;

using Acme.Workforce.Absence.Balance.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class StrictBalanceBudgetEngineChecks
{
    public static void StrictBalanceBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = StrictBalanceBudgetEngine.DefaultStrictBalanceBudgetEngine();
        var item = StrictBalanceBudget.NewStrictBalanceBudget("code-116", "label-805");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictBalanceBudgetEngineKeepsResultsWithinBounds();
    }
}
