namespace Acme.Workforce.Compliance.Tests.Exception.Manifest;

using Acme.Workforce.Compliance.Exception.Manifest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ExceptionAgreementEngineChecks
{
    public static void ExceptionAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionAgreementEngine.DefaultExceptionAgreementEngine(Flags.NewRegistry());
        var item = ExceptionAgreement.NewExceptionAgreement("code-28", "title-808");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionAgreementEngineKeepsResultsWithinBounds();
    }
}
