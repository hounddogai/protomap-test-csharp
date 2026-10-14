namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PulseSurveyMappingChecks
{
    public static void PulseSurveyMappingStartsInNew()
    {
        var item = PulseSurveyMapping.NewPulseSurveyMapping("code-976", "label-253");
        Check.Equal(item.Status, PulseSurveyMappingStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyMappingMovesToQueued()
    {
        var item = PulseSurveyMapping.NewPulseSurveyMapping("code-976", "label-253");
        try
        {
            item.MoveTo(PulseSurveyMappingStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyMappingStatus.Queued, "status");
    }

    public static void PulseSurveyMappingRejectsMovesFromFailed()
    {
        var item = PulseSurveyMapping.NewPulseSurveyMapping("code-976", "label-253");
        item.Status = PulseSurveyMappingStatus.Failed;
        Check.Fails(() => item.MoveTo(PulseSurveyMappingStatus.New), "move");
    }

    public static void RunAll()
    {
        PulseSurveyMappingStartsInNew();
        PulseSurveyMappingMovesToQueued();
        PulseSurveyMappingRejectsMovesFromFailed();
    }
}
