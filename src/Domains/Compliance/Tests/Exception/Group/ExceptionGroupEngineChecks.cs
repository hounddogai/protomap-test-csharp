namespace Acme.Workforce.Compliance.Tests.Exception.Group;

using Acme.Workforce.Compliance.Exception.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ExceptionGroupEngineChecks
{
    public static void ExceptionGroupEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionGroupEngine.DefaultExceptionGroupEngine();
        var item = ExceptionGroup.NewExceptionGroup("code-762", "title-45");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionGroupEngineKeepsResultsWithinBounds();
    }
}
