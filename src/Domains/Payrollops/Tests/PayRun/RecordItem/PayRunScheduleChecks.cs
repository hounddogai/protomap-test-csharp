namespace Acme.Workforce.Payrollops.Tests.PayRun.RecordItem;

using Acme.Workforce.Payrollops.PayRun.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PayRunScheduleChecks
{
    public static void PayRunScheduleStartsInRequested()
    {
        var item = PayRunSchedule.NewPayRunSchedule("code-555", "name-601");
        Check.Equal(item.Status, PayRunScheduleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunScheduleMovesToRevoked()
    {
        var item = PayRunSchedule.NewPayRunSchedule("code-555", "name-601");
        try
        {
            item.MoveTo(PayRunScheduleStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunScheduleStatus.Revoked, "status");
    }

    public static void PayRunScheduleRejectsMovesFromExpired()
    {
        var item = PayRunSchedule.NewPayRunSchedule("code-555", "name-601");
        item.Status = PayRunScheduleStatus.Expired;
        Check.Fails(() => item.MoveTo(PayRunScheduleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PayRunScheduleStartsInRequested();
        PayRunScheduleMovesToRevoked();
        PayRunScheduleRejectsMovesFromExpired();
    }
}
