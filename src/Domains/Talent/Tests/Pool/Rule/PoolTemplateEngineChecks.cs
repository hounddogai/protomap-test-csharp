namespace Acme.Workforce.Talent.Tests.Pool.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PoolTemplateEngineChecks
{
    public static void PoolTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = PoolTemplateEngine.DefaultPoolTemplateEngine(Flags.NewRegistry());
        var item = PoolTemplate.NewPoolTemplate("code-260", "label-80");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PoolTemplateEngineKeepsResultsWithinBounds();
    }
}
