namespace Acme.Workforce.Compliance.Tests.TrainingDue.Mapping;

using Acme.Workforce.Compliance.TrainingDue.Mapping;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DefaultTrainingDueAuditEngineChecks
{
    public static void DefaultTrainingDueAuditEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultTrainingDueAuditEngine.DefaultDefaultTrainingDueAuditEngine(Flags.NewRegistry());
        var item = DefaultTrainingDueAudit.NewDefaultTrainingDueAudit("code-558", "title-74");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultTrainingDueAuditEngineKeepsResultsWithinBounds();
    }
}
