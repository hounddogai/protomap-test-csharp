namespace Acme.Workforce.Orgdesign.Tests.Position.Schedule;

using Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LenientPositionAuditChecks
{
    public static void LenientPositionAuditStartsInOpen()
    {
        var item = LenientPositionAudit.NewLenientPositionAudit("code-206", "label-518");
        Check.Equal(item.Status, LenientPositionAuditStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientPositionAuditMovesToInReview()
    {
        var item = LenientPositionAudit.NewLenientPositionAudit("code-206", "label-518");
        try
        {
            item.MoveTo(LenientPositionAuditStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientPositionAuditStatus.InReview, "status");
    }

    public static void LenientPositionAuditRejectsMovesFromArchived()
    {
        var item = LenientPositionAudit.NewLenientPositionAudit("code-206", "label-518");
        item.Status = LenientPositionAuditStatus.Archived;
        Check.Fails(() => item.MoveTo(LenientPositionAuditStatus.Open), "move");
    }

    public static void RunAll()
    {
        LenientPositionAuditStartsInOpen();
        LenientPositionAuditMovesToInReview();
        LenientPositionAuditRejectsMovesFromArchived();
    }
}
