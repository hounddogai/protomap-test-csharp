namespace Acme.Workforce.Compliance.Tests.TrainingDue.Ledger;

using Acme.Workforce.Compliance.TrainingDue.Ledger;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ScheduledTrainingDueAuditEngineChecks
{
    public static void ScheduledTrainingDueAuditEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledTrainingDueAuditEngine.DefaultScheduledTrainingDueAuditEngine(Flags.NewRegistry());
        var item = ScheduledTrainingDueAudit.NewScheduledTrainingDueAudit("code-827", "name-935");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledTrainingDueAuditEngineKeepsResultsWithinBounds();
    }
}
