namespace Acme.Workforce.Compensation.Tests.EquityGrant.Group;

using Acme.Workforce.Compensation.EquityGrant.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class EquityGrantQuotaChecks
{
    public static void EquityGrantQuotaStartsInRequested()
    {
        var item = EquityGrantQuota.NewEquityGrantQuota("code-672", "name-911");
        Check.Equal(item.Status, EquityGrantQuotaStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantQuotaMovesToExpired()
    {
        var item = EquityGrantQuota.NewEquityGrantQuota("code-672", "name-911");
        try
        {
            item.MoveTo(EquityGrantQuotaStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantQuotaStatus.Expired, "status");
    }

    public static void EquityGrantQuotaRejectsMovesFromExpired()
    {
        var item = EquityGrantQuota.NewEquityGrantQuota("code-672", "name-911");
        item.Status = EquityGrantQuotaStatus.Expired;
        Check.Fails(() => item.MoveTo(EquityGrantQuotaStatus.Requested), "move");
    }

    public static void RunAll()
    {
        EquityGrantQuotaStartsInRequested();
        EquityGrantQuotaMovesToExpired();
        EquityGrantQuotaRejectsMovesFromExpired();
    }
}
