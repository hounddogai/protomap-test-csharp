namespace Acme.Workforce.Assets.Tests.Laptop.Template;

using Acme.Workforce.Assets.Laptop.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LegacyLaptopTemplateChecks
{
    public static void LegacyLaptopTemplateStartsInCreated()
    {
        var item = LegacyLaptopTemplate.NewLegacyLaptopTemplate("code-988", "title-380");
        Check.Equal(item.Status, LegacyLaptopTemplateStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyLaptopTemplateMovesToValidated()
    {
        var item = LegacyLaptopTemplate.NewLegacyLaptopTemplate("code-988", "title-380");
        try
        {
            item.MoveTo(LegacyLaptopTemplateStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyLaptopTemplateStatus.Validated, "status");
    }

    public static void LegacyLaptopTemplateRejectsMovesFromRetired()
    {
        var item = LegacyLaptopTemplate.NewLegacyLaptopTemplate("code-988", "title-380");
        item.Status = LegacyLaptopTemplateStatus.Retired;
        Check.Fails(() => item.MoveTo(LegacyLaptopTemplateStatus.Created), "move");
    }

    public static void RunAll()
    {
        LegacyLaptopTemplateStartsInCreated();
        LegacyLaptopTemplateMovesToValidated();
        LegacyLaptopTemplateRejectsMovesFromRetired();
    }
}
