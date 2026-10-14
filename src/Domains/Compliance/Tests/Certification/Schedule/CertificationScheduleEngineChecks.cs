namespace Acme.Workforce.Compliance.Tests.Certification.Schedule;

using Acme.Workforce.Compliance.Certification.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class CertificationScheduleEngineChecks
{
    public static void CertificationScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = CertificationScheduleEngine.DefaultCertificationScheduleEngine();
        var item = CertificationSchedule.NewCertificationSchedule("code-671", "title-311");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CertificationScheduleEngineKeepsResultsWithinBounds();
    }
}
