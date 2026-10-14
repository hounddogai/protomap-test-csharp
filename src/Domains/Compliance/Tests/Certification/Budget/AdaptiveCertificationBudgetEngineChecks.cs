namespace Acme.Workforce.Compliance.Tests.Certification.Budget;

using Acme.Workforce.Compliance.Certification.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class AdaptiveCertificationBudgetEngineChecks
{
    public static void AdaptiveCertificationBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveCertificationBudgetEngine.DefaultAdaptiveCertificationBudgetEngine();
        var item = AdaptiveCertificationBudget.NewAdaptiveCertificationBudget("code-954", "name-148");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveCertificationBudgetEngineKeepsResultsWithinBounds();
    }
}
