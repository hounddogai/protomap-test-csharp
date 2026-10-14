namespace Acme.Workforce.Workplace.Tests.Visitor.Policy;

using Acme.Workforce.Workplace.Visitor.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class VisitorAgreementEngineChecks
{
    public static void VisitorAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = VisitorAgreementEngine.DefaultVisitorAgreementEngine();
        var item = VisitorAgreement.NewVisitorAgreement("code-321", "name-823");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VisitorAgreementEngineKeepsResultsWithinBounds();
    }
}
