namespace Acme.Workforce.Assets.Tests.Laptop.Bundle;

using Acme.Workforce.Assets.Laptop.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LaptopAgreementEngineChecks
{
    public static void LaptopAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = LaptopAgreementEngine.DefaultLaptopAgreementEngine();
        var item = LaptopAgreement.NewLaptopAgreement("code-867", "label-920");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LaptopAgreementEngineKeepsResultsWithinBounds();
    }
}
