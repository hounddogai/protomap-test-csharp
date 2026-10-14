namespace Acme.Workforce.Talent.Tests.Pool.Rule;

using Acme.Workforce.Talent.Pool.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PoolTemplateChecks
{
    public static void PoolTemplateStartsInProposed()
    {
        var item = PoolTemplate.NewPoolTemplate("code-260", "label-80");
        Check.Equal(item.Status, PoolTemplateStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolTemplateMovesToApproved()
    {
        var item = PoolTemplate.NewPoolTemplate("code-260", "label-80");
        try
        {
            item.MoveTo(PoolTemplateStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolTemplateStatus.Approved, "status");
    }

    public static void PoolTemplateRejectsMovesFromWithdrawn()
    {
        var item = PoolTemplate.NewPoolTemplate("code-260", "label-80");
        item.Status = PoolTemplateStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PoolTemplateStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PoolTemplateStartsInProposed();
        PoolTemplateMovesToApproved();
        PoolTemplateRejectsMovesFromWithdrawn();
    }
}
