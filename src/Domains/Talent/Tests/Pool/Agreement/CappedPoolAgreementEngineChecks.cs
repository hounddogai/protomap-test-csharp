namespace Acme.Workforce.Talent.Tests.Pool.Agreement;

using Acme.Workforce.Talent.Pool.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class CappedPoolAgreementEngineChecks
{
    public static void CappedPoolAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = CappedPoolAgreementEngine.DefaultCappedPoolAgreementEngine();
        var item = CappedPoolAgreement.NewCappedPoolAgreement("code-173", "title-59");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedPoolAgreementEngineKeepsResultsWithinBounds();
    }
}
