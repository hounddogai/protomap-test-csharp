namespace Acme.Workforce.Assets.Tests.Warranty.Template;

using Acme.Workforce.Assets.Warranty.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class WarrantyTemplateChecks
{
    public static void WarrantyTemplateStartsInOpen()
    {
        var item = WarrantyTemplate.NewWarrantyTemplate("code-527", "name-588");
        Check.Equal(item.Status, WarrantyTemplateStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyTemplateMovesToArchived()
    {
        var item = WarrantyTemplate.NewWarrantyTemplate("code-527", "name-588");
        try
        {
            item.MoveTo(WarrantyTemplateStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyTemplateStatus.Archived, "status");
    }

    public static void WarrantyTemplateRejectsMovesFromArchived()
    {
        var item = WarrantyTemplate.NewWarrantyTemplate("code-527", "name-588");
        item.Status = WarrantyTemplateStatus.Archived;
        Check.Fails(() => item.MoveTo(WarrantyTemplateStatus.Open), "move");
    }

    public static void RunAll()
    {
        WarrantyTemplateStartsInOpen();
        WarrantyTemplateMovesToArchived();
        WarrantyTemplateRejectsMovesFromArchived();
    }
}
