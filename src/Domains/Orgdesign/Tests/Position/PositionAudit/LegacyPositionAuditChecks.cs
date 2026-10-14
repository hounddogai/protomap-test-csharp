namespace Acme.Workforce.Orgdesign.Tests.Position.PositionAudit;

using Acme.Workforce.Orgdesign.Position.PositionAudit;

/// <summary>Checks of position audit code, run by the project's runner.</summary>
public static class LegacyPositionAuditChecks
{
    public static void LegacyPositionAuditStartsInNew()
    {
        var item = LegacyPositionAudit.NewLegacyPositionAudit("code-402", "title-197");
        Check.Equal(item.Status, LegacyPositionAuditStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyPositionAuditMovesToQueued()
    {
        var item = LegacyPositionAudit.NewLegacyPositionAudit("code-402", "title-197");
        try
        {
            item.MoveTo(LegacyPositionAuditStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyPositionAuditStatus.Queued, "status");
    }

    public static void LegacyPositionAuditRejectsMovesFromFailed()
    {
        var item = LegacyPositionAudit.NewLegacyPositionAudit("code-402", "title-197");
        item.Status = LegacyPositionAuditStatus.Failed;
        Check.Fails(() => item.MoveTo(LegacyPositionAuditStatus.New), "move");
    }

    public static void RunAll()
    {
        LegacyPositionAuditStartsInNew();
        LegacyPositionAuditMovesToQueued();
        LegacyPositionAuditRejectsMovesFromFailed();
    }
}
