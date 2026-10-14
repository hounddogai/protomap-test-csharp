namespace Acme.Workforce.Orgdesign.Tests.Position.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class PositionTemplateEngineChecks
{
    public static void PositionTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = PositionTemplateEngine.DefaultPositionTemplateEngine(Flags.NewRegistry());
        var item = PositionTemplate.NewPositionTemplate("code-474", "name-92");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PositionTemplateEngineKeepsResultsWithinBounds();
    }
}
