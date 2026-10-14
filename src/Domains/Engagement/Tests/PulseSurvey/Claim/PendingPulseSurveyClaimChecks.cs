namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Claim;

using Acme.Workforce.Engagement.PulseSurvey.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PendingPulseSurveyClaimChecks
{
    public static void PendingPulseSurveyClaimStartsInNew()
    {
        var item = PendingPulseSurveyClaim.NewPendingPulseSurveyClaim("code-9", "name-572");
        Check.Equal(item.Status, PendingPulseSurveyClaimStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingPulseSurveyClaimMovesToQueued()
    {
        var item = PendingPulseSurveyClaim.NewPendingPulseSurveyClaim("code-9", "name-572");
        try
        {
            item.MoveTo(PendingPulseSurveyClaimStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingPulseSurveyClaimStatus.Queued, "status");
    }

    public static void PendingPulseSurveyClaimRejectsMovesFromFailed()
    {
        var item = PendingPulseSurveyClaim.NewPendingPulseSurveyClaim("code-9", "name-572");
        item.Status = PendingPulseSurveyClaimStatus.Failed;
        Check.Fails(() => item.MoveTo(PendingPulseSurveyClaimStatus.New), "move");
    }

    public static void RunAll()
    {
        PendingPulseSurveyClaimStartsInNew();
        PendingPulseSurveyClaimMovesToQueued();
        PendingPulseSurveyClaimRejectsMovesFromFailed();
    }
}
