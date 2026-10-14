namespace Acme.Workforce.Orgdesign.Tests.Position.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PrimaryPositionProfileEngineChecks
{
    public static void PrimaryPositionProfileEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryPositionProfileEngine.DefaultPrimaryPositionProfileEngine(Flags.NewRegistry());
        var item = PrimaryPositionProfile.NewPrimaryPositionProfile("code-880", "title-258");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryPositionProfileEngineKeepsResultsWithinBounds();
    }
}
