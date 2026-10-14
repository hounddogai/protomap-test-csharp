namespace Acme.Workforce.Compensation.Tests.BonusPool.Tier;

using Acme.Workforce.Compensation.BonusPool.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BonusPoolProposalChecks
{
    public static void BonusPoolProposalStartsInCreated()
    {
        var item = BonusPoolProposal.NewBonusPoolProposal("code-976", "title-549");
        Check.Equal(item.Status, BonusPoolProposalStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolProposalMovesToValidated()
    {
        var item = BonusPoolProposal.NewBonusPoolProposal("code-976", "title-549");
        try
        {
            item.MoveTo(BonusPoolProposalStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolProposalStatus.Validated, "status");
    }

    public static void BonusPoolProposalRejectsMovesFromRetired()
    {
        var item = BonusPoolProposal.NewBonusPoolProposal("code-976", "title-549");
        item.Status = BonusPoolProposalStatus.Retired;
        Check.Fails(() => item.MoveTo(BonusPoolProposalStatus.Created), "move");
    }

    public static void RunAll()
    {
        BonusPoolProposalStartsInCreated();
        BonusPoolProposalMovesToValidated();
        BonusPoolProposalRejectsMovesFromRetired();
    }
}
