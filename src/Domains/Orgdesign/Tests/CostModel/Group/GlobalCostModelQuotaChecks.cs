namespace Acme.Workforce.Orgdesign.Tests.CostModel.Group;

using Acme.Workforce.Orgdesign.CostModel.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class GlobalCostModelQuotaChecks
{
    public static void GlobalCostModelQuotaStartsInRequested()
    {
        var item = GlobalCostModelQuota.NewGlobalCostModelQuota("code-726", "name-452");
        Check.Equal(item.Status, GlobalCostModelQuotaStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalCostModelQuotaMovesToGranted()
    {
        var item = GlobalCostModelQuota.NewGlobalCostModelQuota("code-726", "name-452");
        try
        {
            item.MoveTo(GlobalCostModelQuotaStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalCostModelQuotaStatus.Granted, "status");
    }

    public static void GlobalCostModelQuotaRejectsMovesFromExpired()
    {
        var item = GlobalCostModelQuota.NewGlobalCostModelQuota("code-726", "name-452");
        item.Status = GlobalCostModelQuotaStatus.Expired;
        Check.Fails(() => item.MoveTo(GlobalCostModelQuotaStatus.Requested), "move");
    }

    public static void RunAll()
    {
        GlobalCostModelQuotaStartsInRequested();
        GlobalCostModelQuotaMovesToGranted();
        GlobalCostModelQuotaRejectsMovesFromExpired();
    }
}
