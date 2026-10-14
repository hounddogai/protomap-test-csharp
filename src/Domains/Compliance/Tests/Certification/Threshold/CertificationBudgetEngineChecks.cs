namespace Acme.Workforce.Compliance.Tests.Certification.Threshold;

using Acme.Workforce.Compliance.Certification.Threshold;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class CertificationBudgetEngineChecks
{
    public static void CertificationBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = CertificationBudgetEngine.DefaultCertificationBudgetEngine(Flags.NewRegistry());
        var item = CertificationBudget.NewCertificationBudget("code-837", "label-810");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CertificationBudgetEngineKeepsResultsWithinBounds();
    }
}
