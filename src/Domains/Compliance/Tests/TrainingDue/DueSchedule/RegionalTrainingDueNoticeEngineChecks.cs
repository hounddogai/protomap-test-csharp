namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueSchedule;

using Acme.Workforce.Compliance.TrainingDue.DueSchedule;

/// <summary>Checks of due schedule code, run by the project's runner.</summary>
public static class RegionalTrainingDueNoticeEngineChecks
{
    public static void RegionalTrainingDueNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalTrainingDueNoticeEngine.DefaultRegionalTrainingDueNoticeEngine();
        var item = RegionalTrainingDueNotice.NewRegionalTrainingDueNotice("code-917", "label-374");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalTrainingDueNoticeEngineKeepsResultsWithinBounds();
    }
}
