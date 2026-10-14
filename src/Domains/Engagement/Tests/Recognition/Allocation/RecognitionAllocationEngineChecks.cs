namespace Acme.Workforce.Engagement.Tests.Recognition.Allocation;

using Acme.Workforce.Engagement.Recognition.Allocation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class RecognitionAllocationEngineChecks
{
    public static void RecognitionAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = RecognitionAllocationEngine.DefaultRecognitionAllocationEngine(Flags.NewRegistry());
        var item = RecognitionAllocation.NewRecognitionAllocation("code-876", "label-614");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RecognitionAllocationEngineKeepsResultsWithinBounds();
    }
}
