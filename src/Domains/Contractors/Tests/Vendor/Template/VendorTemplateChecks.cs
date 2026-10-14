namespace Acme.Workforce.Contractors.Tests.Vendor.Template;

using Acme.Workforce.Contractors.Vendor.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class VendorTemplateChecks
{
    public static void VendorTemplateStartsInOpen()
    {
        var item = VendorTemplate.NewVendorTemplate("code-570", "name-325");
        Check.Equal(item.Status, VendorTemplateStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VendorTemplateMovesToResolved()
    {
        var item = VendorTemplate.NewVendorTemplate("code-570", "name-325");
        try
        {
            item.MoveTo(VendorTemplateStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VendorTemplateStatus.Resolved, "status");
    }

    public static void VendorTemplateRejectsMovesFromArchived()
    {
        var item = VendorTemplate.NewVendorTemplate("code-570", "name-325");
        item.Status = VendorTemplateStatus.Archived;
        Check.Fails(() => item.MoveTo(VendorTemplateStatus.Open), "move");
    }

    public static void RunAll()
    {
        VendorTemplateStartsInOpen();
        VendorTemplateMovesToResolved();
        VendorTemplateRejectsMovesFromArchived();
    }
}
