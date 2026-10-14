namespace Acme.Workforce.Talent.Tests.SkillProfile.Preference;

using Acme.Workforce.Talent.SkillProfile.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class SkillProfileReservationEngineChecks
{
    public static void SkillProfileReservationEngineKeepsResultsWithinBounds()
    {
        var engine = SkillProfileReservationEngine.DefaultSkillProfileReservationEngine();
        var item = SkillProfileReservation.NewSkillProfileReservation("code-761", "title-23");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SkillProfileReservationEngineKeepsResultsWithinBounds();
    }
}
