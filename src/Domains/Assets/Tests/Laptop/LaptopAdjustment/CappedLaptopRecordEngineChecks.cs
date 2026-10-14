namespace Acme.Workforce.Assets.Tests.Laptop.LaptopAdjustment;

using Acme.Workforce.Assets.Laptop.LaptopAdjustment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of laptop adjustment code, run by the project's runner.</summary>
public static class CappedLaptopRecordEngineChecks
{
    public static void CappedLaptopRecordEngineKeepsResultsWithinBounds()
    {
        var engine = CappedLaptopRecordEngine.DefaultCappedLaptopRecordEngine(Flags.NewRegistry());
        var item = CappedLaptopRecord.NewCappedLaptopRecord("code-941", "name-67");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedLaptopRecordEngineKeepsResultsWithinBounds();
    }
}
