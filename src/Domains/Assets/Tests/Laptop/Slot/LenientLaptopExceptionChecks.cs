namespace Acme.Workforce.Assets.Tests.Laptop.Slot;

using Acme.Workforce.Assets.Laptop.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class LenientLaptopExceptionChecks
{
    public static void LenientLaptopExceptionStartsInRequested()
    {
        var item = LenientLaptopException.NewLenientLaptopException("code-924", "name-926");
        Check.Equal(item.Status, LenientLaptopExceptionStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientLaptopExceptionMovesToRevoked()
    {
        var item = LenientLaptopException.NewLenientLaptopException("code-924", "name-926");
        try
        {
            item.MoveTo(LenientLaptopExceptionStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientLaptopExceptionStatus.Revoked, "status");
    }

    public static void LenientLaptopExceptionRejectsMovesFromExpired()
    {
        var item = LenientLaptopException.NewLenientLaptopException("code-924", "name-926");
        item.Status = LenientLaptopExceptionStatus.Expired;
        Check.Fails(() => item.MoveTo(LenientLaptopExceptionStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LenientLaptopExceptionStartsInRequested();
        LenientLaptopExceptionMovesToRevoked();
        LenientLaptopExceptionRejectsMovesFromExpired();
    }
}
