namespace Acme.Workforce.Compensation.Tests.BonusPool.Variance;

using Acme.Workforce.Compensation.BonusPool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class ManualBonusPoolProposalChecks
{
    public static void ManualBonusPoolProposalStartsInNew()
    {
        var item = ManualBonusPoolProposal.NewManualBonusPoolProposal("code-949", "name-690");
        Check.Equal(item.Status, ManualBonusPoolProposalStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualBonusPoolProposalMovesToDone()
    {
        var item = ManualBonusPoolProposal.NewManualBonusPoolProposal("code-949", "name-690");
        try
        {
            item.MoveTo(ManualBonusPoolProposalStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualBonusPoolProposalStatus.Done, "status");
    }

    public static void ManualBonusPoolProposalRejectsMovesFromFailed()
    {
        var item = ManualBonusPoolProposal.NewManualBonusPoolProposal("code-949", "name-690");
        item.Status = ManualBonusPoolProposalStatus.Failed;
        Check.Fails(() => item.MoveTo(ManualBonusPoolProposalStatus.New), "move");
    }

    public static void RunAll()
    {
        ManualBonusPoolProposalStartsInNew();
        ManualBonusPoolProposalMovesToDone();
        ManualBonusPoolProposalRejectsMovesFromFailed();
    }
}
