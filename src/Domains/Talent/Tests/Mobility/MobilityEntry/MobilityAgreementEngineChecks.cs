namespace Acme.Workforce.Talent.Tests.Mobility.MobilityEntry;

using Acme.Workforce.Talent.Mobility.MobilityEntry;

/// <summary>Checks of mobility entry code, run by the project's runner.</summary>
public static class MobilityAgreementEngineChecks
{
    public static void MobilityAgreementEngineKeepsResultsWithinBounds()
    {
        var engine = MobilityAgreementEngine.DefaultMobilityAgreementEngine();
        var item = MobilityAgreement.NewMobilityAgreement("code-898", "label-509");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        MobilityAgreementEngineKeepsResultsWithinBounds();
    }
}
