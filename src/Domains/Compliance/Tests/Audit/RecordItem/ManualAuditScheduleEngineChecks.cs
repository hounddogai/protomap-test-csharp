namespace Acme.Workforce.Compliance.Tests.Audit.RecordItem;

using Acme.Workforce.Compliance.Audit.RecordItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class ManualAuditScheduleEngineChecks
{
    public static void ManualAuditScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = ManualAuditScheduleEngine.DefaultManualAuditScheduleEngine(Flags.NewRegistry());
        var item = ManualAuditSchedule.NewManualAuditSchedule("code-419", "title-803");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualAuditScheduleEngineKeepsResultsWithinBounds();
    }
}
