namespace Acme.Workforce.Compliance.Tests.Exception.Request;

using Acme.Workforce.Compliance.Exception.Request;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class DefaultExceptionDigestEngineChecks
{
    public static void DefaultExceptionDigestEngineExtraKeepsResultsWithinBounds()
    {
        var engine = DefaultExceptionDigestEngine.DefaultDefaultExceptionDigestEngine(Flags.NewRegistry());
        var item = DefaultExceptionDigest.NewDefaultExceptionDigest("code-533", "label-981");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultExceptionDigestEngineExtraKeepsResultsWithinBounds();
    }
}
