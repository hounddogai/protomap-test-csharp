namespace Acme.Workforce.Compliance.Tests.Exception.Batch;

using Acme.Workforce.Compliance.Exception.Batch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class CappedExceptionAllocationEngineChecks
{
    public static void CappedExceptionAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = CappedExceptionAllocationEngine.DefaultCappedExceptionAllocationEngine(Flags.NewRegistry());
        var item = CappedExceptionAllocation.NewCappedExceptionAllocation("code-409", "label-55");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedExceptionAllocationEngineKeepsResultsWithinBounds();
    }
}
