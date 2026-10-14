namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Entry;

using Acme.Workforce.Engagement.PulseSurvey.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class ManualPulseSurveySummaryChecks
{
    public static void ManualPulseSurveySummaryStartsInDraft()
    {
        var item = ManualPulseSurveySummary.NewManualPulseSurveySummary("code-207", "label-363");
        Check.Equal(item.Status, ManualPulseSurveySummaryStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualPulseSurveySummaryMovesToClosed()
    {
        var item = ManualPulseSurveySummary.NewManualPulseSurveySummary("code-207", "label-363");
        try
        {
            item.MoveTo(ManualPulseSurveySummaryStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualPulseSurveySummaryStatus.Closed, "status");
    }

    public static void ManualPulseSurveySummaryRejectsMovesFromClosed()
    {
        var item = ManualPulseSurveySummary.NewManualPulseSurveySummary("code-207", "label-363");
        item.Status = ManualPulseSurveySummaryStatus.Closed;
        Check.Fails(() => item.MoveTo(ManualPulseSurveySummaryStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ManualPulseSurveySummaryStartsInDraft();
        ManualPulseSurveySummaryMovesToClosed();
        ManualPulseSurveySummaryRejectsMovesFromClosed();
    }
}
