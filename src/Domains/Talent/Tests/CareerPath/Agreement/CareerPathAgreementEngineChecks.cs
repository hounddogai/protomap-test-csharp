namespace Acme.Workforce.Talent.Tests.CareerPath.Agreement;

using Acme.Workforce.Talent.CareerPath.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class CareerPathAgreementEngineChecks
{
    public static void CareerPathAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = CareerPathAgreementEngine.DefaultCareerPathAgreementEngine();
        var item = CareerPathAgreement.NewCareerPathAgreement("code-552", "name-118");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CareerPathAgreementEngineKeepsResultsWithinBounds();
    }
}
