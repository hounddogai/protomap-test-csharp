namespace Acme.Workforce.Payrollops.Tests.Deduction.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DeductionNoticeEngineChecks
{
    public static void DeductionNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = DeductionNoticeEngine.DefaultDeductionNoticeEngine(Flags.NewRegistry());
        var item = DeductionNotice.NewDeductionNotice("code-369", "label-395");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DeductionNoticeEngineKeepsResultsWithinBounds();
    }
}
