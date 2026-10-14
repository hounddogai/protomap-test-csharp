namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelPolicy;

using Acme.Workforce.Orgdesign.CostModel.ModelPolicy;

/// <summary>Checks of model policy code, run by the project's runner.</summary>
public static class CostModelQuotaChecks
{
    public static void CostModelQuotaStartsInProposed()
    {
        var item = CostModelQuota.NewCostModelQuota("code-393", "name-44");
        Check.Equal(item.Status, CostModelQuotaStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelQuotaMovesToApproved()
    {
        var item = CostModelQuota.NewCostModelQuota("code-393", "name-44");
        try
        {
            item.MoveTo(CostModelQuotaStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelQuotaStatus.Approved, "status");
    }

    public static void CostModelQuotaRejectsMovesFromWithdrawn()
    {
        var item = CostModelQuota.NewCostModelQuota("code-393", "name-44");
        item.Status = CostModelQuotaStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CostModelQuotaStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CostModelQuotaStartsInProposed();
        CostModelQuotaMovesToApproved();
        CostModelQuotaRejectsMovesFromWithdrawn();
    }
}
