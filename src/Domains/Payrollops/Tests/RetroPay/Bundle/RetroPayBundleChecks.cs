namespace Acme.Workforce.Payrollops.Tests.RetroPay.Bundle;

using Acme.Workforce.Payrollops.RetroPay.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class RetroPayBundleChecks
{
    public static void RetroPayBundleStartsInDraft()
    {
        var item = RetroPayBundle.NewRetroPayBundle("code-138", "label-448");
        Check.Equal(item.Status, RetroPayBundleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayBundleMovesToPending()
    {
        var item = RetroPayBundle.NewRetroPayBundle("code-138", "label-448");
        try
        {
            item.MoveTo(RetroPayBundleStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayBundleStatus.Pending, "status");
    }

    public static void RetroPayBundleRejectsMovesFromClosed()
    {
        var item = RetroPayBundle.NewRetroPayBundle("code-138", "label-448");
        item.Status = RetroPayBundleStatus.Closed;
        Check.Fails(() => item.MoveTo(RetroPayBundleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RetroPayBundleStartsInDraft();
        RetroPayBundleMovesToPending();
        RetroPayBundleRejectsMovesFromClosed();
    }
}
