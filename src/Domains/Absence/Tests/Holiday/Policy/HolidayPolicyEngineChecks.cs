namespace Acme.Workforce.Absence.Tests.Holiday.Policy;

using Acme.Workforce.Absence.Holiday.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class HolidayPolicyEngineChecks
{
    public static void HolidayPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = HolidayPolicyEngine.DefaultHolidayPolicyEngine();
        var item = HolidayPolicy.NewHolidayPolicy("code-111", "name-247");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HolidayPolicyEngineKeepsResultsWithinBounds();
    }
}
