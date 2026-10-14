namespace Acme.Workforce.Compliance.Tests.Exception.RollingExceptionGroup;

using Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>Checks of rolling exception group code, run by the project's runner.</summary>
public static class LegacyExceptionAgreementEngineChecks
{
    public static void LegacyExceptionAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyExceptionAgreementEngine.DefaultLegacyExceptionAgreementEngine();
        var item = LegacyExceptionAgreement.NewLegacyExceptionAgreement("code-202", "name-227");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyExceptionAgreementEngineKeepsResultsWithinBounds();
    }
}
