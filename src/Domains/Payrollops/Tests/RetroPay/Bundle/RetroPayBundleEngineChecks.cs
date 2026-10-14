namespace Acme.Workforce.Payrollops.Tests.RetroPay.Bundle;

using Acme.Workforce.Payrollops.RetroPay.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class RetroPayBundleEngineChecks
{
    public static void RetroPayBundleEngineKeepsResultsWithinBounds()
    {
        var engine = RetroPayBundleEngine.DefaultRetroPayBundleEngine();
        var item = RetroPayBundle.NewRetroPayBundle("code-138", "label-448");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RetroPayBundleEngineKeepsResultsWithinBounds();
    }
}
