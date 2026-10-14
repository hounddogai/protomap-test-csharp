namespace Acme.Workforce.Orgdesign.Tests.Position.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredPositionTemplateEngineChecks
{
    public static void TieredPositionTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = TieredPositionTemplateEngine.DefaultTieredPositionTemplateEngine(Flags.NewRegistry());
        var item = TieredPositionTemplate.NewTieredPositionTemplate("code-381", "label-484");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredPositionTemplateEngineKeepsResultsWithinBounds();
    }
}
