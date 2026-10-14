namespace Acme.Workforce.Compensation.Tests.Adjustment.Agreement;

using Acme.Workforce.Compensation.Adjustment.Agreement;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AdjustmentAgreementEngineChecks
{
    public static void AdjustmentAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = AdjustmentAgreementEngine.DefaultAdjustmentAgreementEngine(Flags.NewRegistry());
        var item = AdjustmentAgreement.NewAdjustmentAgreement("code-290", "name-80");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdjustmentAgreementEngineKeepsResultsWithinBounds();
    }
}
