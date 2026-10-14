namespace Acme.Workforce.Compliance.Tests.Exception.Request;

using Acme.Workforce.Compliance.Exception.Request;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class ExceptionDigestEngineChecks
{
    public static void ExceptionDigestEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionDigestEngine.DefaultExceptionDigestEngine(Flags.NewRegistry());
        var item = ExceptionDigest.NewExceptionDigest("code-412", "name-333");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionDigestEngineKeepsResultsWithinBounds();
    }
}
