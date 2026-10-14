namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgException;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.ReorgException;

/// <summary>Checks of reorg exception code, run by the project's runner.</summary>
public static class LenientReorgSlotEngineChecks
{
    public static void LenientReorgSlotEngineKeepsResultsWithinBounds()
    {
        var engine = LenientReorgSlotEngine.DefaultLenientReorgSlotEngine(Flags.NewRegistry());
        var item = LenientReorgSlot.NewLenientReorgSlot("code-830", "label-30");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientReorgSlotEngineKeepsResultsWithinBounds();
    }
}
