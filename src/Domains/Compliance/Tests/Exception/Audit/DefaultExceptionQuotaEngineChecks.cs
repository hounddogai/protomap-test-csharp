namespace Acme.Workforce.Compliance.Tests.Exception.Audit;

using Acme.Workforce.Compliance.Exception.Audit;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class DefaultExceptionQuotaEngineChecks
{
    public static void DefaultExceptionQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultExceptionQuotaEngine.DefaultDefaultExceptionQuotaEngine(Flags.NewRegistry());
        var item = DefaultExceptionQuota.NewDefaultExceptionQuota("code-293", "title-62");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultExceptionQuotaEngineKeepsResultsWithinBounds();
    }
}
