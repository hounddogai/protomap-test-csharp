namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueDraft;

using Acme.Workforce.Compliance.TrainingDue.DueDraft;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of due draft code, run by the project's runner.</summary>
public static class LegacyTrainingDueSegmentEngineChecks
{
    public static void LegacyTrainingDueSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyTrainingDueSegmentEngine.DefaultLegacyTrainingDueSegmentEngine(Flags.NewRegistry());
        var item = LegacyTrainingDueSegment.NewLegacyTrainingDueSegment("code-822", "label-968");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyTrainingDueSegmentEngineKeepsResultsWithinBounds();
    }
}
