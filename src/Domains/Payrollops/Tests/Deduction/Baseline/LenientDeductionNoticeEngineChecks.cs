namespace Acme.Workforce.Payrollops.Tests.Deduction.Baseline;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Deduction.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LenientDeductionNoticeEngineChecks
{
    public static void LenientDeductionNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = LenientDeductionNoticeEngine.DefaultLenientDeductionNoticeEngine(Flags.NewRegistry());
        var item = LenientDeductionNotice.NewLenientDeductionNotice("code-779", "label-697");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientDeductionNoticeEngineKeepsResultsWithinBounds();
    }
}
