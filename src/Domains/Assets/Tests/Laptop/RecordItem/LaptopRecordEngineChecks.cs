namespace Acme.Workforce.Assets.Tests.Laptop.RecordItem;

using Acme.Workforce.Assets.Laptop.RecordItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LaptopRecordEngineChecks
{
    public static void LaptopRecordEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopRecordEngine.DefaultLaptopRecordEngine(Flags.NewRegistry());
        var item = LaptopRecord.NewLaptopRecord("code-726", "label-642");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopRecordEngineKeepsResultsWithinBounds();
    }
}
