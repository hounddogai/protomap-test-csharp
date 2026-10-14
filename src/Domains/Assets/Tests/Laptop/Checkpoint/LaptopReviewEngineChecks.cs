namespace Acme.Workforce.Assets.Tests.Laptop.Checkpoint;

using Acme.Workforce.Assets.Laptop.Checkpoint;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class LaptopReviewEngineChecks
{
    public static void LaptopReviewEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopReviewEngine.DefaultLaptopReviewEngine(Flags.NewRegistry());
        var item = LaptopReview.NewLaptopReview("code-591", "title-743");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopReviewEngineKeepsResultsWithinBounds();
    }
}
