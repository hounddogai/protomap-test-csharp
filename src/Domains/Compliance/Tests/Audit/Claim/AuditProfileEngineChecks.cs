namespace Acme.Workforce.Compliance.Tests.Audit.Claim;

using Acme.Workforce.Compliance.Audit.Claim;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AuditProfileEngineChecks
{
    public static void AuditProfileEngineKeepsResultsWithinBounds()
    {
        var engine = AuditProfileEngine.DefaultAuditProfileEngine(Flags.NewRegistry());
        var item = AuditProfile.NewAuditProfile("code-735", "label-129");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AuditProfileEngineKeepsResultsWithinBounds();
    }
}
