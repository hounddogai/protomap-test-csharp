namespace Acme.Workforce.Assets.Tests.Refresh.Schedule;

using Acme.Workforce.Assets.Refresh.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class PrimaryRefreshRequestChecks
{
    public static void PrimaryRefreshRequestStartsInCreated()
    {
        var item = PrimaryRefreshRequest.NewPrimaryRefreshRequest("code-970", "name-882");
        Check.Equal(item.Status, PrimaryRefreshRequestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryRefreshRequestMovesToPublished()
    {
        var item = PrimaryRefreshRequest.NewPrimaryRefreshRequest("code-970", "name-882");
        try
        {
            item.MoveTo(PrimaryRefreshRequestStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryRefreshRequestStatus.Published, "status");
    }

    public static void PrimaryRefreshRequestRejectsMovesFromRetired()
    {
        var item = PrimaryRefreshRequest.NewPrimaryRefreshRequest("code-970", "name-882");
        item.Status = PrimaryRefreshRequestStatus.Retired;
        Check.Fails(() => item.MoveTo(PrimaryRefreshRequestStatus.Created), "move");
    }

    public static void RunAll()
    {
        PrimaryRefreshRequestStartsInCreated();
        PrimaryRefreshRequestMovesToPublished();
        PrimaryRefreshRequestRejectsMovesFromRetired();
    }
}
