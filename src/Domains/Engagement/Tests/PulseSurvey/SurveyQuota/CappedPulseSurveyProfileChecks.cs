namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyQuota;

using Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

/// <summary>Checks of survey quota code, run by the project's runner.</summary>
public static class CappedPulseSurveyProfileChecks
{
    public static void CappedPulseSurveyProfileStartsInNew()
    {
        var item = CappedPulseSurveyProfile.NewCappedPulseSurveyProfile("code-437", "title-372");
        Check.Equal(item.Status, CappedPulseSurveyProfileStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedPulseSurveyProfileMovesToRunning()
    {
        var item = CappedPulseSurveyProfile.NewCappedPulseSurveyProfile("code-437", "title-372");
        try
        {
            item.MoveTo(CappedPulseSurveyProfileStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedPulseSurveyProfileStatus.Running, "status");
    }

    public static void CappedPulseSurveyProfileRejectsMovesFromFailed()
    {
        var item = CappedPulseSurveyProfile.NewCappedPulseSurveyProfile("code-437", "title-372");
        item.Status = CappedPulseSurveyProfileStatus.Failed;
        Check.Fails(() => item.MoveTo(CappedPulseSurveyProfileStatus.New), "move");
    }

    public static void RunAll()
    {
        CappedPulseSurveyProfileStartsInNew();
        CappedPulseSurveyProfileMovesToRunning();
        CappedPulseSurveyProfileRejectsMovesFromFailed();
    }
}
