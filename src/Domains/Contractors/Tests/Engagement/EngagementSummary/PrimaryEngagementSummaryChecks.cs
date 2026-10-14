namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;

/// <summary>Checks of engagement summary code, run by the project's runner.</summary>
public static class PrimaryEngagementSummaryChecks
{
    public static void PrimaryEngagementSummaryStartsInCreated()
    {
        var item = PrimaryEngagementSummary.NewPrimaryEngagementSummary("code-255", "title-268");
        Check.Equal(item.Status, PrimaryEngagementSummaryStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryEngagementSummaryMovesToValidated()
    {
        var item = PrimaryEngagementSummary.NewPrimaryEngagementSummary("code-255", "title-268");
        try
        {
            item.MoveTo(PrimaryEngagementSummaryStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryEngagementSummaryStatus.Validated, "status");
    }

    public static void PrimaryEngagementSummaryRejectsMovesFromRetired()
    {
        var item = PrimaryEngagementSummary.NewPrimaryEngagementSummary("code-255", "title-268");
        item.Status = PrimaryEngagementSummaryStatus.Retired;
        Check.Fails(() => item.MoveTo(PrimaryEngagementSummaryStatus.Created), "move");
    }

    public static void RunAll()
    {
        PrimaryEngagementSummaryStartsInCreated();
        PrimaryEngagementSummaryMovesToValidated();
        PrimaryEngagementSummaryRejectsMovesFromRetired();
    }
}
