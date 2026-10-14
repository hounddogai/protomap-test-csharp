namespace Acme.Workforce.Payrollops.Tests.PayRun.Request;

using Acme.Workforce.Payrollops.PayRun.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LegacyPayRunRequestChecks
{
    public static void LegacyPayRunRequestStartsInDraft()
    {
        var item = LegacyPayRunRequest.NewLegacyPayRunRequest("code-564", "title-577");
        Check.Equal(item.Status, LegacyPayRunRequestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyPayRunRequestMovesToActive()
    {
        var item = LegacyPayRunRequest.NewLegacyPayRunRequest("code-564", "title-577");
        try
        {
            item.MoveTo(LegacyPayRunRequestStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyPayRunRequestStatus.Active, "status");
    }

    public static void LegacyPayRunRequestRejectsMovesFromClosed()
    {
        var item = LegacyPayRunRequest.NewLegacyPayRunRequest("code-564", "title-577");
        item.Status = LegacyPayRunRequestStatus.Closed;
        Check.Fails(() => item.MoveTo(LegacyPayRunRequestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LegacyPayRunRequestStartsInDraft();
        LegacyPayRunRequestMovesToActive();
        LegacyPayRunRequestRejectsMovesFromClosed();
    }
}
