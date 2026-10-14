namespace Acme.Workforce.Compliance.Tests.TrainingDue.Notice;

using Acme.Workforce.Compliance.TrainingDue.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TrainingDueAuditEngineChecks
{
    public static void TrainingDueAuditEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueAuditEngine.DefaultTrainingDueAuditEngine(Flags.NewRegistry());
        var item = TrainingDueAudit.NewTrainingDueAudit("code-898", "title-406");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueAuditEngineKeepsResultsWithinBounds();
    }
}
