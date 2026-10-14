namespace Acme.Workforce.Compensation.Tests.EquityGrant.Quota;

using Acme.Workforce.Compensation.EquityGrant.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ScheduledEquityGrantQuotaChecks
{
    public static void ScheduledEquityGrantQuotaStartsInCreated()
    {
        var item = ScheduledEquityGrantQuota.NewScheduledEquityGrantQuota("code-60", "title-629");
        Check.Equal(item.Status, ScheduledEquityGrantQuotaStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledEquityGrantQuotaMovesToValidated()
    {
        var item = ScheduledEquityGrantQuota.NewScheduledEquityGrantQuota("code-60", "title-629");
        try
        {
            item.MoveTo(ScheduledEquityGrantQuotaStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledEquityGrantQuotaStatus.Validated, "status");
    }

    public static void ScheduledEquityGrantQuotaRejectsMovesFromRetired()
    {
        var item = ScheduledEquityGrantQuota.NewScheduledEquityGrantQuota("code-60", "title-629");
        item.Status = ScheduledEquityGrantQuotaStatus.Retired;
        Check.Fails(() => item.MoveTo(ScheduledEquityGrantQuotaStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScheduledEquityGrantQuotaStartsInCreated();
        ScheduledEquityGrantQuotaMovesToValidated();
        ScheduledEquityGrantQuotaRejectsMovesFromRetired();
    }
}
