namespace Acme.Workforce.Compliance.Tests.Certification.Review;

using Acme.Workforce.Compliance.Certification.Review;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class DefaultCertificationBudgetEngineChecks
{
    public static void DefaultCertificationBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultCertificationBudgetEngine.DefaultDefaultCertificationBudgetEngine(Flags.NewRegistry());
        var item = DefaultCertificationBudget.NewDefaultCertificationBudget("code-621", "label-58");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultCertificationBudgetEngineKeepsResultsWithinBounds();
    }
}
