namespace Acme.Workforce.Workplace.Tests.Visitor.Assignment;

using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VisitorSummaryChecks
{
    public static void VisitorSummaryStartsInDraft()
    {
        var item = VisitorSummary.NewVisitorSummary("code-638", "title-746");
        Check.Equal(item.Status, VisitorSummaryStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorSummaryMovesToActive()
    {
        var item = VisitorSummary.NewVisitorSummary("code-638", "title-746");
        try
        {
            item.MoveTo(VisitorSummaryStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorSummaryStatus.Active, "status");
    }

    public static void VisitorSummaryRejectsMovesFromClosed()
    {
        var item = VisitorSummary.NewVisitorSummary("code-638", "title-746");
        item.Status = VisitorSummaryStatus.Closed;
        Check.Fails(() => item.MoveTo(VisitorSummaryStatus.Draft), "move");
    }

    public static void RunAll()
    {
        VisitorSummaryStartsInDraft();
        VisitorSummaryMovesToActive();
        VisitorSummaryRejectsMovesFromClosed();
    }
}
