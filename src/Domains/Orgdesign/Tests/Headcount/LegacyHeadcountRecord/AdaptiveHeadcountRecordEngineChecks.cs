namespace Acme.Workforce.Orgdesign.Tests.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

/// <summary>Checks of legacy headcount record code, run by the project's runner.</summary>
public static class AdaptiveHeadcountRecordEngineChecks
{
    public static void AdaptiveHeadcountRecordEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveHeadcountRecordEngine.DefaultAdaptiveHeadcountRecordEngine(Flags.NewRegistry());
        var item = AdaptiveHeadcountRecord.NewAdaptiveHeadcountRecord("code-830", "label-533");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveHeadcountRecordEngineKeepsResultsWithinBounds();
    }
}
