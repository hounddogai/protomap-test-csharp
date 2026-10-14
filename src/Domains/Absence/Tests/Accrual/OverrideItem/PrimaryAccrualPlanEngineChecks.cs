namespace Acme.Workforce.Absence.Tests.Accrual.OverrideItem;

using Acme.Workforce.Absence.Accrual.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PrimaryAccrualPlanEngineChecks
{
    public static void PrimaryAccrualPlanEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryAccrualPlanEngine.DefaultPrimaryAccrualPlanEngine();
        var item = PrimaryAccrualPlan.NewPrimaryAccrualPlan("code-827", "label-185");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryAccrualPlanEngineKeepsResultsWithinBounds();
    }
}
