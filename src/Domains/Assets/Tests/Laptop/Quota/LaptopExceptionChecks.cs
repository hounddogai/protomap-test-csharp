namespace Acme.Workforce.Assets.Tests.Laptop.Quota;

using Acme.Workforce.Assets.Laptop.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class LaptopExceptionChecks
{
    public static void LaptopExceptionStartsInOpen()
    {
        var item = LaptopException.NewLaptopException("code-154", "label-748");
        Check.Equal(item.Status, LaptopExceptionStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LaptopExceptionMovesToArchived()
    {
        var item = LaptopException.NewLaptopException("code-154", "label-748");
        try
        {
            item.MoveTo(LaptopExceptionStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LaptopExceptionStatus.Archived, "status");
    }

    public static void LaptopExceptionRejectsMovesFromArchived()
    {
        var item = LaptopException.NewLaptopException("code-154", "label-748");
        item.Status = LaptopExceptionStatus.Archived;
        Check.Fails(() => item.MoveTo(LaptopExceptionStatus.Open), "move");
    }

    public static void RunAll()
    {
        LaptopExceptionStartsInOpen();
        LaptopExceptionMovesToArchived();
        LaptopExceptionRejectsMovesFromArchived();
    }
}
