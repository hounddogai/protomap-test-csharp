namespace Acme.Workforce.Compensation.Tests.BonusPool.Reservation;

using Acme.Workforce.Compensation.BonusPool.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BonusPoolAgreementEngineChecks
{
    public static void BonusPoolAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = BonusPoolAgreementEngine.DefaultBonusPoolAgreementEngine();
        var item = BonusPoolAgreement.NewBonusPoolAgreement("code-631", "title-974");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BonusPoolAgreementEngineKeepsResultsWithinBounds();
    }
}
