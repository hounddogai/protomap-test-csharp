namespace Acme.Workforce.Absence.Tests.Holiday.HolidayQuota;

using Acme.Workforce.Absence.Holiday.HolidayQuota;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of holiday quota code, run by the project's runner.</summary>
public static class PendingHolidayQuotaEngineChecks
{
    public static void PendingHolidayQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = PendingHolidayQuotaEngine.DefaultPendingHolidayQuotaEngine(Flags.NewRegistry());
        var item = PendingHolidayQuota.NewPendingHolidayQuota("code-149", "title-210");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingHolidayQuotaEngineKeepsResultsWithinBounds();
    }
}
