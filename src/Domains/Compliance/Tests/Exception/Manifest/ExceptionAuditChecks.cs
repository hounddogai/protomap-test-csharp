namespace Acme.Workforce.Compliance.Tests.Exception.Manifest;

using Acme.Workforce.Compliance.Exception.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ExceptionAuditChecks
{
    public static void ExceptionAuditStartsInIdle()
    {
        var item = ExceptionAudit.NewExceptionAudit("code-109", "label-264");
        Check.Equal(item.Status, ExceptionAuditStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionAuditMovesToWarming()
    {
        var item = ExceptionAudit.NewExceptionAudit("code-109", "label-264");
        try
        {
            item.MoveTo(ExceptionAuditStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionAuditStatus.Warming, "status");
    }

    public static void ExceptionAuditRejectsMovesFromStopped()
    {
        var item = ExceptionAudit.NewExceptionAudit("code-109", "label-264");
        item.Status = ExceptionAuditStatus.Stopped;
        Check.Fails(() => item.MoveTo(ExceptionAuditStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ExceptionAuditStartsInIdle();
        ExceptionAuditMovesToWarming();
        ExceptionAuditRejectsMovesFromStopped();
    }
}
