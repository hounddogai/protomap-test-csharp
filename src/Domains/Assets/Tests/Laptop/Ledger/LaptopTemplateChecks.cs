namespace Acme.Workforce.Assets.Tests.Laptop.Ledger;

using Acme.Workforce.Assets.Laptop.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class LaptopTemplateChecks
{
    public static void LaptopTemplateStartsInNew()
    {
        var item = LaptopTemplate.NewLaptopTemplate("code-84", "title-859");
        Check.Equal(item.Status, LaptopTemplateStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopTemplateMovesToRunning()
    {
        var item = LaptopTemplate.NewLaptopTemplate("code-84", "title-859");
        try
        {
            item.MoveTo(LaptopTemplateStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopTemplateStatus.Running, "status");
    }

    public static void LaptopTemplateRejectsMovesFromFailed()
    {
        var item = LaptopTemplate.NewLaptopTemplate("code-84", "title-859");
        item.Status = LaptopTemplateStatus.Failed;
        Check.Fails(() => item.MoveTo(LaptopTemplateStatus.New), "move");
    }

    public static void RunAll()
    {
        LaptopTemplateStartsInNew();
        LaptopTemplateMovesToRunning();
        LaptopTemplateRejectsMovesFromFailed();
    }
}
