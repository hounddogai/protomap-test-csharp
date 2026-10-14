namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorOverride;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.VisitorOverride;

/// <summary>Checks of visitor override code, run by the project's runner.</summary>
public static class PrimaryVisitorOverrideEngineChecks
{
    public static void PrimaryVisitorOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryVisitorOverrideEngine.DefaultPrimaryVisitorOverrideEngine(Flags.NewRegistry());
        var item = PrimaryVisitorOverride.NewPrimaryVisitorOverride("code-353", "name-807");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryVisitorOverrideEngineKeepsResultsWithinBounds();
    }
}
