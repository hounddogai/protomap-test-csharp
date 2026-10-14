namespace Acme.Workforce.Assets.Tests.Laptop.Mapping;

using Acme.Workforce.Assets.Laptop.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DefaultLaptopWindowChecks
{
    public static void DefaultLaptopWindowStartsInCreated()
    {
        var item = DefaultLaptopWindow.NewDefaultLaptopWindow("code-130", "label-387");
        Check.Equal(item.Status, DefaultLaptopWindowStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultLaptopWindowMovesToValidated()
    {
        var item = DefaultLaptopWindow.NewDefaultLaptopWindow("code-130", "label-387");
        try
        {
            item.MoveTo(DefaultLaptopWindowStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultLaptopWindowStatus.Validated, "status");
    }

    public static void DefaultLaptopWindowRejectsMovesFromRetired()
    {
        var item = DefaultLaptopWindow.NewDefaultLaptopWindow("code-130", "label-387");
        item.Status = DefaultLaptopWindowStatus.Retired;
        Check.Fails(() => item.MoveTo(DefaultLaptopWindowStatus.Created), "move");
    }

    public static void RunAll()
    {
        DefaultLaptopWindowStartsInCreated();
        DefaultLaptopWindowMovesToValidated();
        DefaultLaptopWindowRejectsMovesFromRetired();
    }
}
