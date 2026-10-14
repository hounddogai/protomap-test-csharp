namespace Acme.Workforce.Compliance.Tests.Attestation.Milestone;

using Acme.Workforce.Compliance.Attestation.Milestone;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class AttestationTemplateEngineChecks
{
    public static void AttestationTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = AttestationTemplateEngine.DefaultAttestationTemplateEngine(Flags.NewRegistry());
        var item = AttestationTemplate.NewAttestationTemplate("code-293", "title-275");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AttestationTemplateEngineKeepsResultsWithinBounds();
    }
}
