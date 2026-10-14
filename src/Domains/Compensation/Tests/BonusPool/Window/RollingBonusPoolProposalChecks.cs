namespace Acme.Workforce.Compensation.Tests.BonusPool.Window;

using Acme.Workforce.Compensation.BonusPool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RollingBonusPoolProposalChecks
{
    public static void RollingBonusPoolProposalStartsInNew()
    {
        var item = RollingBonusPoolProposal.NewRollingBonusPoolProposal("code-837", "title-267");
        Check.Equal(item.Status, RollingBonusPoolProposalStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingBonusPoolProposalMovesToRunning()
    {
        var item = RollingBonusPoolProposal.NewRollingBonusPoolProposal("code-837", "title-267");
        try
        {
            item.MoveTo(RollingBonusPoolProposalStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingBonusPoolProposalStatus.Running, "status");
    }

    public static void RollingBonusPoolProposalRejectsMovesFromFailed()
    {
        var item = RollingBonusPoolProposal.NewRollingBonusPoolProposal("code-837", "title-267");
        item.Status = RollingBonusPoolProposalStatus.Failed;
        Check.Fails(() => item.MoveTo(RollingBonusPoolProposalStatus.New), "move");
    }

    public static void RunAll()
    {
        RollingBonusPoolProposalStartsInNew();
        RollingBonusPoolProposalMovesToRunning();
        RollingBonusPoolProposalRejectsMovesFromFailed();
    }
}
