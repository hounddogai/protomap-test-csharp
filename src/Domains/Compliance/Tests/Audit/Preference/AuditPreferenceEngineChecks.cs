namespace Acme.Workforce.Compliance.Tests.Audit.Preference;

using Acme.Workforce.Compliance.Audit.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class AuditPreferenceEngineChecks
{
    public static void AuditPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = AuditPreferenceEngine.DefaultAuditPreferenceEngine();
        var item = AuditPreference.NewAuditPreference("code-709", "label-617");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AuditPreferenceEngineKeepsResultsWithinBounds();
    }
}
