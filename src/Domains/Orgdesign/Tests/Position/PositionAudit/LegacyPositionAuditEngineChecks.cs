namespace Acme.Workforce.Orgdesign.Tests.Position.PositionAudit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.PositionAudit;

/// <summary>Checks of position audit code, run by the project's runner.</summary>
public static class LegacyPositionAuditEngineChecks
{
    public static void LegacyPositionAuditEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyPositionAuditEngine.DefaultLegacyPositionAuditEngine(Flags.NewRegistry());
        var item = LegacyPositionAudit.NewLegacyPositionAudit("code-402", "title-197");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyPositionAuditEngineKeepsResultsWithinBounds();
    }
}
