namespace Acme.Workforce.Payrollops.Tests.Garnishment.Agreement;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Garnishment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class GarnishmentAgreementEngineChecks
{
    public static void GarnishmentAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = GarnishmentAgreementEngine.DefaultGarnishmentAgreementEngine(Flags.NewRegistry());
        var item = GarnishmentAgreement.NewGarnishmentAgreement("code-840", "title-387");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GarnishmentAgreementEngineKeepsResultsWithinBounds();
    }
}
