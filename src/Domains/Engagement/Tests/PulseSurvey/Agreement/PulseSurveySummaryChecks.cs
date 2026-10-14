namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Agreement;

using Acme.Workforce.Engagement.PulseSurvey.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class PulseSurveySummaryChecks
{
    public static void PulseSurveySummaryStartsInNew()
    {
        var item = PulseSurveySummary.NewPulseSurveySummary("code-237", "name-286");
        Check.Equal(item.Status, PulseSurveySummaryStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveySummaryMovesToQueued()
    {
        var item = PulseSurveySummary.NewPulseSurveySummary("code-237", "name-286");
        try
        {
            item.MoveTo(PulseSurveySummaryStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveySummaryStatus.Queued, "status");
    }

    public static void PulseSurveySummaryRejectsMovesFromFailed()
    {
        var item = PulseSurveySummary.NewPulseSurveySummary("code-237", "name-286");
        item.Status = PulseSurveySummaryStatus.Failed;
        Check.Fails(() => item.MoveTo(PulseSurveySummaryStatus.New), "move");
    }

    public static void RunAll()
    {
        PulseSurveySummaryStartsInNew();
        PulseSurveySummaryMovesToQueued();
        PulseSurveySummaryRejectsMovesFromFailed();
    }
}
