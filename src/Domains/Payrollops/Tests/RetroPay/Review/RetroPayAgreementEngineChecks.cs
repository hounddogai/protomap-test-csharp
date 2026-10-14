namespace Acme.Workforce.Payrollops.Tests.RetroPay.Review;

using Acme.Workforce.Payrollops.RetroPay.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RetroPayAgreementEngineChecks
{
    public static void RetroPayAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = RetroPayAgreementEngine.DefaultRetroPayAgreementEngine();
        var item = RetroPayAgreement.NewRetroPayAgreement("code-470", "name-498");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RetroPayAgreementEngineKeepsResultsWithinBounds();
    }
}
