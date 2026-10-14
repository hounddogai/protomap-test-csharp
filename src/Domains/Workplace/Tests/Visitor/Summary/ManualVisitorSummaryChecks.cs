namespace Acme.Workforce.Workplace.Tests.Visitor.Summary;

using Acme.Workforce.Workplace.Visitor.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ManualVisitorSummaryChecks
{
    public static void ManualVisitorSummaryStartsInOpen()
    {
        var item = ManualVisitorSummary.NewManualVisitorSummary("code-973", "name-694");
        Check.Equal(item.Status, ManualVisitorSummaryStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualVisitorSummaryMovesToInReview()
    {
        var item = ManualVisitorSummary.NewManualVisitorSummary("code-973", "name-694");
        try
        {
            item.MoveTo(ManualVisitorSummaryStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualVisitorSummaryStatus.InReview, "status");
    }

    public static void ManualVisitorSummaryRejectsMovesFromArchived()
    {
        var item = ManualVisitorSummary.NewManualVisitorSummary("code-973", "name-694");
        item.Status = ManualVisitorSummaryStatus.Archived;
        Check.Fails(() => item.MoveTo(ManualVisitorSummaryStatus.Open), "move");
    }

    public static void RunAll()
    {
        ManualVisitorSummaryStartsInOpen();
        ManualVisitorSummaryMovesToInReview();
        ManualVisitorSummaryRejectsMovesFromArchived();
    }
}
