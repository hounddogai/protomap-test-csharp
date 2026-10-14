namespace Acme.Workforce.Workplace.Tests.AccessBadge.Request;

using Acme.Workforce.Workplace.AccessBadge.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class PendingAccessBadgeRequestChecks
{
    public static void PendingAccessBadgeRequestStartsInOpen()
    {
        var item = PendingAccessBadgeRequest.NewPendingAccessBadgeRequest("code-382", "name-684");
        Check.Equal(item.Status, PendingAccessBadgeRequestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingAccessBadgeRequestMovesToArchived()
    {
        var item = PendingAccessBadgeRequest.NewPendingAccessBadgeRequest("code-382", "name-684");
        try
        {
            item.MoveTo(PendingAccessBadgeRequestStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingAccessBadgeRequestStatus.Archived, "status");
    }

    public static void PendingAccessBadgeRequestRejectsMovesFromArchived()
    {
        var item = PendingAccessBadgeRequest.NewPendingAccessBadgeRequest("code-382", "name-684");
        item.Status = PendingAccessBadgeRequestStatus.Archived;
        Check.Fails(() => item.MoveTo(PendingAccessBadgeRequestStatus.Open), "move");
    }

    public static void RunAll()
    {
        PendingAccessBadgeRequestStartsInOpen();
        PendingAccessBadgeRequestMovesToArchived();
        PendingAccessBadgeRequestRejectsMovesFromArchived();
    }
}
