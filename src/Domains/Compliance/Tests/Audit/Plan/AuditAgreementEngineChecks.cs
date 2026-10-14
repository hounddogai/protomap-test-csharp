namespace Acme.Workforce.Compliance.Tests.Audit.Plan;

using Acme.Workforce.Compliance.Audit.Plan;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class AuditAgreementEngineChecks
{
    public static void AuditAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = AuditAgreementEngine.DefaultAuditAgreementEngine(Flags.NewRegistry());
        var item = AuditAgreement.NewAuditAgreement("code-139", "name-677");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AuditAgreementEngineKeepsResultsWithinBounds();
    }
}
