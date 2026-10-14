namespace Acme.Workforce.Compliance.Tests.PolicyAck.AckQuota;

using Acme.Workforce.Compliance.PolicyAck.AckQuota;

/// <summary>Checks of ack quota code, run by the project's runner.</summary>
public static class ScheduledPolicyAckQuotaChecks
{
    public static void ScheduledPolicyAckQuotaStartsInPlanned()
    {
        var item = ScheduledPolicyAckQuota.NewScheduledPolicyAckQuota("code-458", "name-923");
        Check.Equal(item.Status, ScheduledPolicyAckQuotaStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPolicyAckQuotaMovesToScheduled()
    {
        var item = ScheduledPolicyAckQuota.NewScheduledPolicyAckQuota("code-458", "name-923");
        try
        {
            item.MoveTo(ScheduledPolicyAckQuotaStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPolicyAckQuotaStatus.Scheduled, "status");
    }

    public static void ScheduledPolicyAckQuotaRejectsMovesFromCancelled()
    {
        var item = ScheduledPolicyAckQuota.NewScheduledPolicyAckQuota("code-458", "name-923");
        item.Status = ScheduledPolicyAckQuotaStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ScheduledPolicyAckQuotaStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ScheduledPolicyAckQuotaStartsInPlanned();
        ScheduledPolicyAckQuotaMovesToScheduled();
        ScheduledPolicyAckQuotaRejectsMovesFromCancelled();
    }
}
