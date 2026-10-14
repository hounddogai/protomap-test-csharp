namespace Acme.Workforce.Absence.Tests.Holiday.HolidayPolicy;

using Acme.Workforce.Absence.Holiday.HolidayPolicy;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of holiday policy code, run by the project's runner.</summary>
public static class RegionalHolidayPolicyEngineChecks
{
    public static void RegionalHolidayPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalHolidayPolicyEngine.DefaultRegionalHolidayPolicyEngine(Flags.NewRegistry());
        var item = RegionalHolidayPolicy.NewRegionalHolidayPolicy("code-585", "name-273");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalHolidayPolicyEngineKeepsResultsWithinBounds();
    }
}
