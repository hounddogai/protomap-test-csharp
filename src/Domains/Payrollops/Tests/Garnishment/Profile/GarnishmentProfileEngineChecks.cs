namespace Acme.Workforce.Payrollops.Tests.Garnishment.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Garnishment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class GarnishmentProfileEngineChecks
{
    public static void GarnishmentProfileEngineKeepsResultsWithinBounds()
    {
        var engine = GarnishmentProfileEngine.DefaultGarnishmentProfileEngine(Flags.NewRegistry());
        var item = GarnishmentProfile.NewGarnishmentProfile("code-423", "title-546");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GarnishmentProfileEngineKeepsResultsWithinBounds();
    }
}
