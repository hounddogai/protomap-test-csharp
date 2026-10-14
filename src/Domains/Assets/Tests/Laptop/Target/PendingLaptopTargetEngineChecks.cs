namespace Acme.Workforce.Assets.Tests.Laptop.Target;

using Acme.Workforce.Assets.Laptop.Target;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PendingLaptopTargetEngineChecks
{
    public static void PendingLaptopTargetEngineKeepsResultsWithinBounds()
    {
        var engine = PendingLaptopTargetEngine.DefaultPendingLaptopTargetEngine(Flags.NewRegistry());
        var item = PendingLaptopTarget.NewPendingLaptopTarget("code-118", "label-24");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PendingLaptopTargetEngineKeepsResultsWithinBounds();
    }
}
