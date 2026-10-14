namespace Acme.Workforce.Contractors.Tests.Offboarding.Mapping;

using Acme.Workforce.Contractors.Offboarding.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class OffboardingRecordEngineChecks
{
    public static void OffboardingRecordEngineKeepsResultsWithinBounds()
    {
        var engine = OffboardingRecordEngine.DefaultOffboardingRecordEngine();
        var item = OffboardingRecord.NewOffboardingRecord("code-774", "label-840");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OffboardingRecordEngineKeepsResultsWithinBounds();
    }
}
