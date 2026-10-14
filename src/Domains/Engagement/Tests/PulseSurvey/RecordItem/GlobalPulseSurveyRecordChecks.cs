namespace Acme.Workforce.Engagement.Tests.PulseSurvey.RecordItem;

using Acme.Workforce.Engagement.PulseSurvey.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class GlobalPulseSurveyRecordChecks
{
    public static void GlobalPulseSurveyRecordStartsInCreated()
    {
        var item = GlobalPulseSurveyRecord.NewGlobalPulseSurveyRecord("code-931", "name-823");
        Check.Equal(item.Status, GlobalPulseSurveyRecordStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalPulseSurveyRecordMovesToPublished()
    {
        var item = GlobalPulseSurveyRecord.NewGlobalPulseSurveyRecord("code-931", "name-823");
        try
        {
            item.MoveTo(GlobalPulseSurveyRecordStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalPulseSurveyRecordStatus.Published, "status");
    }

    public static void GlobalPulseSurveyRecordRejectsMovesFromRetired()
    {
        var item = GlobalPulseSurveyRecord.NewGlobalPulseSurveyRecord("code-931", "name-823");
        item.Status = GlobalPulseSurveyRecordStatus.Retired;
        Check.Fails(() => item.MoveTo(GlobalPulseSurveyRecordStatus.Created), "move");
    }

    public static void RunAll()
    {
        GlobalPulseSurveyRecordStartsInCreated();
        GlobalPulseSurveyRecordMovesToPublished();
        GlobalPulseSurveyRecordRejectsMovesFromRetired();
    }
}
