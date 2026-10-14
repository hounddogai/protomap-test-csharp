namespace Acme.Workforce.Compliance.Tests.Attestation.Template;

using Acme.Workforce.Compliance.Attestation.Template;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class DefaultAttestationTemplateEngineChecks
{
    public static void DefaultAttestationTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultAttestationTemplateEngine.DefaultDefaultAttestationTemplateEngine(Flags.NewRegistry());
        var item = DefaultAttestationTemplate.NewDefaultAttestationTemplate("code-673", "title-971");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultAttestationTemplateEngineKeepsResultsWithinBounds();
    }
}
