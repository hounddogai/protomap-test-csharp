namespace Acme.Workforce.Contractors.Tests.Offboarding.Request;

using Acme.Workforce.Contractors.Offboarding.Request;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class OffboardingRequestEngineChecks
{
    public static void OffboardingRequestEngineKeepsResultsWithinBounds()
    {
        var engine = OffboardingRequestEngine.DefaultOffboardingRequestEngine(Flags.NewRegistry());
        var item = OffboardingRequest.NewOffboardingRequest("code-544", "title-462");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OffboardingRequestEngineKeepsResultsWithinBounds();
    }
}
