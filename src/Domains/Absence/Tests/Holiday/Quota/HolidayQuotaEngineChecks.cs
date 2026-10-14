namespace Acme.Workforce.Absence.Tests.Holiday.Quota;

using Acme.Workforce.Absence.Holiday.Quota;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class HolidayQuotaEngineChecks
{
    public static void HolidayQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = HolidayQuotaEngine.DefaultHolidayQuotaEngine(Flags.NewRegistry());
        var item = HolidayQuota.NewHolidayQuota("code-672", "name-66");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidayQuotaEngineKeepsResultsWithinBounds();
    }
}
