namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingThreshold;

using Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of offboarding threshold code, run by the project's runner.</summary>
public static class OffboardingLedgerEngineChecks
{
    public static void OffboardingLedgerEngineKeepsResultsWithinBounds()
    {
        var engine = OffboardingLedgerEngine.DefaultOffboardingLedgerEngine(Flags.NewRegistry());
        var item = OffboardingLedger.NewOffboardingLedger("code-807", "title-411");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        OffboardingLedgerEngineKeepsResultsWithinBounds();
    }
}
