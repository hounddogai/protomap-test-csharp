namespace Acme.Workforce.Compliance.Tests.Exception.Template;

using Acme.Workforce.Compliance.Exception.Template;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ExceptionTemplateEngineChecks
{
    public static void ExceptionTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionTemplateEngine.DefaultExceptionTemplateEngine(Flags.NewRegistry());
        var item = ExceptionTemplate.NewExceptionTemplate("code-325", "title-116");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionTemplateEngineKeepsResultsWithinBounds();
    }
}
