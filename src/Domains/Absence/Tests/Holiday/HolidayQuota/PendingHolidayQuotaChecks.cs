namespace Acme.Workforce.Absence.Tests.Holiday.HolidayQuota;

using Acme.Workforce.Absence.Holiday.HolidayQuota;

/// <summary>Checks of holiday quota code, run by the project's runner.</summary>
public static class PendingHolidayQuotaChecks
{
    public static void PendingHolidayQuotaStartsInIdle()
    {
        var item = PendingHolidayQuota.NewPendingHolidayQuota("code-149", "title-210");
        Check.Equal(item.Status, PendingHolidayQuotaStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingHolidayQuotaMovesToDraining()
    {
        var item = PendingHolidayQuota.NewPendingHolidayQuota("code-149", "title-210");
        try
        {
            item.MoveTo(PendingHolidayQuotaStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingHolidayQuotaStatus.Draining, "status");
    }

    public static void PendingHolidayQuotaRejectsMovesFromStopped()
    {
        var item = PendingHolidayQuota.NewPendingHolidayQuota("code-149", "title-210");
        item.Status = PendingHolidayQuotaStatus.Stopped;
        Check.Fails(() => item.MoveTo(PendingHolidayQuotaStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PendingHolidayQuotaStartsInIdle();
        PendingHolidayQuotaMovesToDraining();
        PendingHolidayQuotaRejectsMovesFromStopped();
    }
}
