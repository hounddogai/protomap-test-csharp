namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class CostModelNoticeChecks
{
    public static void CostModelNoticeStartsInProposed()
    {
        var item = CostModelNotice.NewCostModelNotice("code-465", "title-703");
        Check.Equal(item.Status, CostModelNoticeStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelNoticeMovesToWithdrawn()
    {
        var item = CostModelNotice.NewCostModelNotice("code-465", "title-703");
        try
        {
            item.MoveTo(CostModelNoticeStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelNoticeStatus.Withdrawn, "status");
    }

    public static void CostModelNoticeRejectsMovesFromWithdrawn()
    {
        var item = CostModelNotice.NewCostModelNotice("code-465", "title-703");
        item.Status = CostModelNoticeStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CostModelNoticeStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CostModelNoticeStartsInProposed();
        CostModelNoticeMovesToWithdrawn();
        CostModelNoticeRejectsMovesFromWithdrawn();
    }
}
