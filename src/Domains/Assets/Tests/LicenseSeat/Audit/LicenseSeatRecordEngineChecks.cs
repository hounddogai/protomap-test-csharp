namespace Acme.Workforce.Assets.Tests.LicenseSeat.Audit;

using Acme.Workforce.Assets.LicenseSeat.Audit;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LicenseSeatRecordEngineChecks
{
    public static void LicenseSeatRecordEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatRecordEngine.DefaultLicenseSeatRecordEngine(Flags.NewRegistry());
        var item = LicenseSeatRecord.NewLicenseSeatRecord("code-274", "name-824");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatRecordEngineKeepsResultsWithinBounds();
    }
}
