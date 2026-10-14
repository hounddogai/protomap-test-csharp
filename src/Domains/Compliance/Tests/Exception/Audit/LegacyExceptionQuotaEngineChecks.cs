namespace Acme.Workforce.Compliance.Tests.Exception.Audit;

using Acme.Workforce.Compliance.Exception.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LegacyExceptionQuotaEngineChecks
{
    public static void LegacyExceptionQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyExceptionQuotaEngine.DefaultLegacyExceptionQuotaEngine();
        var item = LegacyExceptionQuota.NewLegacyExceptionQuota("code-780", "title-762");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyExceptionQuotaEngineKeepsResultsWithinBounds();
    }
}
