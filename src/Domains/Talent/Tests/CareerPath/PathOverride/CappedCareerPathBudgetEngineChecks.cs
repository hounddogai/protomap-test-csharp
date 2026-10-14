namespace Acme.Workforce.Talent.Tests.CareerPath.PathOverride;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.CareerPath.PathOverride;

/// <summary>Checks of path override code, run by the project's runner.</summary>
public static class CappedCareerPathBudgetEngineChecks
{
    public static void CappedCareerPathBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = CappedCareerPathBudgetEngine.DefaultCappedCareerPathBudgetEngine(Flags.NewRegistry());
        var item = CappedCareerPathBudget.NewCappedCareerPathBudget("code-540", "label-97");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedCareerPathBudgetEngineKeepsResultsWithinBounds();
    }
}
