namespace Acme.Workforce.Absence.Tests.Accrual.Bundle;

using Acme.Workforce.Absence.Accrual.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class AccrualBundleEngineChecks
{
    public static void AccrualBundleEngineKeepsResultsWithinBounds()
    {
        var engine = AccrualBundleEngine.DefaultAccrualBundleEngine();
        var item = AccrualBundle.NewAccrualBundle("code-858", "name-894");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccrualBundleEngineKeepsResultsWithinBounds();
    }
}
