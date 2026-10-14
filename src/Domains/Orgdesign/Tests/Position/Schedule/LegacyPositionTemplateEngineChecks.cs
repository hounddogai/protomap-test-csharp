namespace Acme.Workforce.Orgdesign.Tests.Position.Schedule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LegacyPositionTemplateEngineChecks
{
    public static void LegacyPositionTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyPositionTemplateEngine.DefaultLegacyPositionTemplateEngine(Flags.NewRegistry());
        var item = LegacyPositionTemplate.NewLegacyPositionTemplate("code-371", "name-815");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyPositionTemplateEngineKeepsResultsWithinBounds();
    }
}
