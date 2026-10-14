namespace Acme.Workforce.Absence.Tests.Accrual.Group;

using Acme.Workforce.Absence.Accrual.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class WeightedAccrualGroupEngineChecks
{
    public static void WeightedAccrualGroupEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedAccrualGroupEngine.DefaultWeightedAccrualGroupEngine();
        var item = WeightedAccrualGroup.NewWeightedAccrualGroup("code-167", "title-153");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedAccrualGroupEngineKeepsResultsWithinBounds();
    }
}
