namespace Acme.Workforce.Compliance.Tests.TrainingDue.OverrideItem;

using Acme.Workforce.Compliance.TrainingDue.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class TieredTrainingDueAuditEngineChecks
{
    public static void TieredTrainingDueAuditEngineKeepsResultsWithinBounds()
    {
        var engine = TieredTrainingDueAuditEngine.DefaultTieredTrainingDueAuditEngine(Flags.NewRegistry());
        var item = TieredTrainingDueAudit.NewTieredTrainingDueAudit("code-557", "name-559");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredTrainingDueAuditEngineKeepsResultsWithinBounds();
    }
}
