namespace Acme.Workforce.Assets.Tests.LicenseSeat.Baseline;

using Acme.Workforce.Assets.LicenseSeat.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FallbackLicenseSeatWindowChecks
{
    public static void FallbackLicenseSeatWindowStartsInPlanned()
    {
        var item = FallbackLicenseSeatWindow.NewFallbackLicenseSeatWindow("code-543", "name-96");
        Check.Equal(item.Status, FallbackLicenseSeatWindowStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackLicenseSeatWindowMovesToInProgress()
    {
        var item = FallbackLicenseSeatWindow.NewFallbackLicenseSeatWindow("code-543", "name-96");
        try
        {
            item.MoveTo(FallbackLicenseSeatWindowStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackLicenseSeatWindowStatus.InProgress, "status");
    }

    public static void FallbackLicenseSeatWindowRejectsMovesFromCancelled()
    {
        var item = FallbackLicenseSeatWindow.NewFallbackLicenseSeatWindow("code-543", "name-96");
        item.Status = FallbackLicenseSeatWindowStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FallbackLicenseSeatWindowStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FallbackLicenseSeatWindowStartsInPlanned();
        FallbackLicenseSeatWindowMovesToInProgress();
        FallbackLicenseSeatWindowRejectsMovesFromCancelled();
    }
}
