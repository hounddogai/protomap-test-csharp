namespace Acme.Workforce.Assets.Tests.Laptop.Checkpoint;

using Acme.Workforce.Assets.Laptop.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class FallbackLaptopTemplateChecks
{
    public static void FallbackLaptopTemplateStartsInCreated()
    {
        var item = FallbackLaptopTemplate.NewFallbackLaptopTemplate("code-295", "label-591");
        Check.Equal(item.Status, FallbackLaptopTemplateStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackLaptopTemplateMovesToValidated()
    {
        var item = FallbackLaptopTemplate.NewFallbackLaptopTemplate("code-295", "label-591");
        try
        {
            item.MoveTo(FallbackLaptopTemplateStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackLaptopTemplateStatus.Validated, "status");
    }

    public static void FallbackLaptopTemplateRejectsMovesFromRetired()
    {
        var item = FallbackLaptopTemplate.NewFallbackLaptopTemplate("code-295", "label-591");
        item.Status = FallbackLaptopTemplateStatus.Retired;
        Check.Fails(() => item.MoveTo(FallbackLaptopTemplateStatus.Created), "move");
    }

    public static void RunAll()
    {
        FallbackLaptopTemplateStartsInCreated();
        FallbackLaptopTemplateMovesToValidated();
        FallbackLaptopTemplateRejectsMovesFromRetired();
    }
}
