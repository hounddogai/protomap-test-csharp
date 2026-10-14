namespace Acme.Workforce.Compensation.Tests.PayBand.Exception;

using Acme.Workforce.Compensation.PayBand.Exception;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class TieredPayBandDraftEngineChecks
{
    public static void TieredPayBandDraftEngineKeepsResultsWithinBounds()
    {
        var engine = TieredPayBandDraftEngine.DefaultTieredPayBandDraftEngine(Flags.NewRegistry());
        var item = TieredPayBandDraft.NewTieredPayBandDraft("code-118", "label-107");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredPayBandDraftEngineKeepsResultsWithinBounds();
    }
}
