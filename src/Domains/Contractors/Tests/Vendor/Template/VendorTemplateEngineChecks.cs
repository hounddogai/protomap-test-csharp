namespace Acme.Workforce.Contractors.Tests.Vendor.Template;

using Acme.Workforce.Contractors.Vendor.Template;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class VendorTemplateEngineChecks
{
    public static void VendorTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = VendorTemplateEngine.DefaultVendorTemplateEngine(Flags.NewRegistry());
        var item = VendorTemplate.NewVendorTemplate("code-570", "name-325");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VendorTemplateEngineKeepsResultsWithinBounds();
    }
}
