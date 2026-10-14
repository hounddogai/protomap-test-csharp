namespace Acme.Workforce.Assets.Tests.Laptop.Snapshot;

using Acme.Workforce.Assets.Laptop.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ManualLaptopExceptionChecks
{
    public static void ManualLaptopExceptionStartsInCreated()
    {
        var item = ManualLaptopException.NewManualLaptopException("code-500", "label-878");
        Check.Equal(item.Status, ManualLaptopExceptionStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualLaptopExceptionMovesToPublished()
    {
        var item = ManualLaptopException.NewManualLaptopException("code-500", "label-878");
        try
        {
            item.MoveTo(ManualLaptopExceptionStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualLaptopExceptionStatus.Published, "status");
    }

    public static void ManualLaptopExceptionRejectsMovesFromRetired()
    {
        var item = ManualLaptopException.NewManualLaptopException("code-500", "label-878");
        item.Status = ManualLaptopExceptionStatus.Retired;
        Check.Fails(() => item.MoveTo(ManualLaptopExceptionStatus.Created), "move");
    }

    public static void RunAll()
    {
        ManualLaptopExceptionStartsInCreated();
        ManualLaptopExceptionMovesToPublished();
        ManualLaptopExceptionRejectsMovesFromRetired();
    }
}
