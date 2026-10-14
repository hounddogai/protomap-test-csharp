namespace Acme.Workforce.Orgdesign.Tests.Position.Audit;

using Acme.Workforce.Orgdesign.Position.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PositionAuditChecks
{
    public static void PositionAuditStartsInDraft()
    {
        var item = PositionAudit.NewPositionAudit("code-569", "title-765");
        Check.Equal(item.Status, PositionAuditStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionAuditMovesToActive()
    {
        var item = PositionAudit.NewPositionAudit("code-569", "title-765");
        try
        {
            item.MoveTo(PositionAuditStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionAuditStatus.Active, "status");
    }

    public static void PositionAuditRejectsMovesFromClosed()
    {
        var item = PositionAudit.NewPositionAudit("code-569", "title-765");
        item.Status = PositionAuditStatus.Closed;
        Check.Fails(() => item.MoveTo(PositionAuditStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PositionAuditStartsInDraft();
        PositionAuditMovesToActive();
        PositionAuditRejectsMovesFromClosed();
    }
}
