namespace Acme.Workforce.Compensation.Tests.EquityGrant.RecordItem;

using Acme.Workforce.Compensation.EquityGrant.RecordItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class EquityGrantRecordEngineChecks
{
    public static void EquityGrantRecordEngineKeepsResultsWithinBounds()
    {
        var engine = EquityGrantRecordEngine.DefaultEquityGrantRecordEngine(Flags.NewRegistry());
        var item = EquityGrantRecord.NewEquityGrantRecord("code-424", "label-934");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EquityGrantRecordEngineKeepsResultsWithinBounds();
    }
}
