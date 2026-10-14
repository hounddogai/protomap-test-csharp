namespace Acme.Workforce.Orgdesign.Tests.Headcount.Notice;

using Acme.Workforce.Orgdesign.Headcount.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class HeadcountBudgetEngineChecks
{
    public static void HeadcountBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = HeadcountBudgetEngine.DefaultHeadcountBudgetEngine();
        var item = HeadcountBudget.NewHeadcountBudget("code-573", "title-524");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HeadcountBudgetEngineKeepsResultsWithinBounds();
    }
}
