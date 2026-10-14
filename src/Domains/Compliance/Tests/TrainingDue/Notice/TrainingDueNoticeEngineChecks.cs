namespace Acme.Workforce.Compliance.Tests.TrainingDue.Notice;

using Acme.Workforce.Compliance.TrainingDue.Notice;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TrainingDueNoticeEngineChecks
{
    public static void TrainingDueNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueNoticeEngine.DefaultTrainingDueNoticeEngine(Flags.NewRegistry());
        var item = TrainingDueNotice.NewTrainingDueNotice("code-101", "label-714");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueNoticeEngineKeepsResultsWithinBounds();
    }
}
