namespace Acme.Workforce.Talent.Tests.CareerPath.Profile;

using Acme.Workforce.Talent.CareerPath.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RollingCareerPathProfileEngineChecks
{
    public static void RollingCareerPathProfileEngineKeepsResultsWithinBounds()
    {
        var engine = RollingCareerPathProfileEngine.DefaultRollingCareerPathProfileEngine();
        var item = RollingCareerPathProfile.NewRollingCareerPathProfile("code-638", "name-465");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingCareerPathProfileEngineKeepsResultsWithinBounds();
    }
}
