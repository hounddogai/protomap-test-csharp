namespace Acme.Workforce.Assets.Tests.Refresh.RefreshRequest;

using Acme.Workforce.Assets.Refresh.RefreshRequest;

/// <summary>Checks of refresh request code, run by the project's runner.</summary>
public static class LenientRefreshRequestChecks
{
    public static void LenientRefreshRequestStartsInCreated()
    {
        var item = LenientRefreshRequest.NewLenientRefreshRequest("code-174", "title-23");
        Check.Equal(item.Status, LenientRefreshRequestStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientRefreshRequestMovesToPublished()
    {
        var item = LenientRefreshRequest.NewLenientRefreshRequest("code-174", "title-23");
        try
        {
            item.MoveTo(LenientRefreshRequestStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientRefreshRequestStatus.Published, "status");
    }

    public static void LenientRefreshRequestRejectsMovesFromRetired()
    {
        var item = LenientRefreshRequest.NewLenientRefreshRequest("code-174", "title-23");
        item.Status = LenientRefreshRequestStatus.Retired;
        Check.Fails(() => item.MoveTo(LenientRefreshRequestStatus.Created), "move");
    }

    public static void RunAll()
    {
        LenientRefreshRequestStartsInCreated();
        LenientRefreshRequestMovesToPublished();
        LenientRefreshRequestRejectsMovesFromRetired();
    }
}
