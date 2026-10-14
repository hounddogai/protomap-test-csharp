namespace Acme.Workforce.Absence.Tests.Holiday.Quota;

using Acme.Workforce.Absence.Holiday.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class HolidayQuotaChecks
{
    public static void HolidayQuotaStartsInIdle()
    {
        var item = HolidayQuota.NewHolidayQuota("code-672", "name-66");
        Check.Equal(item.Status, HolidayQuotaStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayQuotaMovesToWarming()
    {
        var item = HolidayQuota.NewHolidayQuota("code-672", "name-66");
        try
        {
            item.MoveTo(HolidayQuotaStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayQuotaStatus.Warming, "status");
    }

    public static void HolidayQuotaRejectsMovesFromStopped()
    {
        var item = HolidayQuota.NewHolidayQuota("code-672", "name-66");
        item.Status = HolidayQuotaStatus.Stopped;
        Check.Fails(() => item.MoveTo(HolidayQuotaStatus.Idle), "move");
    }

    public static void RunAll()
    {
        HolidayQuotaStartsInIdle();
        HolidayQuotaMovesToWarming();
        HolidayQuotaRejectsMovesFromStopped();
    }
}
