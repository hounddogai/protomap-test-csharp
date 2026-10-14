namespace Acme.Workforce.Assets.Tests.Warranty.Bundle;

using Acme.Workforce.Assets.Warranty.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class WarrantyExceptionChecks
{
    public static void WarrantyExceptionStartsInIdle()
    {
        var item = WarrantyException.NewWarrantyException("code-534", "label-643");
        Check.Equal(item.Status, WarrantyExceptionStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WarrantyExceptionMovesToWarming()
    {
        var item = WarrantyException.NewWarrantyException("code-534", "label-643");
        try
        {
            item.MoveTo(WarrantyExceptionStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WarrantyExceptionStatus.Warming, "status");
    }

    public static void WarrantyExceptionRejectsMovesFromStopped()
    {
        var item = WarrantyException.NewWarrantyException("code-534", "label-643");
        item.Status = WarrantyExceptionStatus.Stopped;
        Check.Fails(() => item.MoveTo(WarrantyExceptionStatus.Idle), "move");
    }

    public static void RunAll()
    {
        WarrantyExceptionStartsInIdle();
        WarrantyExceptionMovesToWarming();
        WarrantyExceptionRejectsMovesFromStopped();
    }
}
