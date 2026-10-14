namespace Acme.Workforce.Compliance.Tests.Attestation.Window;

using Acme.Workforce.Compliance.Attestation.Window;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class AttestationWindowEngineChecks
{
    public static void AttestationWindowEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationWindowEngine.DefaultAttestationWindowEngine(Flags.NewRegistry());
        var item = AttestationWindow.NewAttestationWindow("code-18", "title-223");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationWindowEngineKeepsResultsWithinBounds();
    }
}
