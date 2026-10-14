namespace Acme.Workforce.Assets.Tests.Refresh.Request;

using Acme.Workforce.Assets.Refresh.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RefreshAuditChecks
{
    public static void RefreshAuditStartsInCreated()
    {
        var item = RefreshAudit.NewRefreshAudit("code-587", "title-538");
        Check.Equal(item.Status, RefreshAuditStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshAuditMovesToValidated()
    {
        var item = RefreshAudit.NewRefreshAudit("code-587", "title-538");
        try
        {
            item.MoveTo(RefreshAuditStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshAuditStatus.Validated, "status");
    }

    public static void RefreshAuditRejectsMovesFromRetired()
    {
        var item = RefreshAudit.NewRefreshAudit("code-587", "title-538");
        item.Status = RefreshAuditStatus.Retired;
        Check.Fails(() => item.MoveTo(RefreshAuditStatus.Created), "move");
    }

    public static void RunAll()
    {
        RefreshAuditStartsInCreated();
        RefreshAuditMovesToValidated();
        RefreshAuditRejectsMovesFromRetired();
    }
}
