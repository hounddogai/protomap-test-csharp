namespace Acme.Workforce.Orgdesign.Tests.Headcount.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Headcount.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class HeadcountAgreementEngineChecks
{
    public static void HeadcountAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = HeadcountAgreementEngine.DefaultHeadcountAgreementEngine(Flags.NewRegistry());
        var item = HeadcountAgreement.NewHeadcountAgreement("code-372", "label-801");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        HeadcountAgreementEngineKeepsResultsWithinBounds();
    }
}
