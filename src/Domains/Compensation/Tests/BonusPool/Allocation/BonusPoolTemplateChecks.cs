namespace Acme.Workforce.Compensation.Tests.BonusPool.Allocation;

using Acme.Workforce.Compensation.BonusPool.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class BonusPoolTemplateChecks
{
    public static void BonusPoolTemplateStartsInProposed()
    {
        var item = BonusPoolTemplate.NewBonusPoolTemplate("code-381", "title-954");
        Check.Equal(item.Status, BonusPoolTemplateStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolTemplateMovesToApproved()
    {
        var item = BonusPoolTemplate.NewBonusPoolTemplate("code-381", "title-954");
        try
        {
            item.MoveTo(BonusPoolTemplateStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolTemplateStatus.Approved, "status");
    }

    public static void BonusPoolTemplateRejectsMovesFromWithdrawn()
    {
        var item = BonusPoolTemplate.NewBonusPoolTemplate("code-381", "title-954");
        item.Status = BonusPoolTemplateStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BonusPoolTemplateStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BonusPoolTemplateStartsInProposed();
        BonusPoolTemplateMovesToApproved();
        BonusPoolTemplateRejectsMovesFromWithdrawn();
    }
}
