namespace Acme.Workforce.Payrollops.Tests.RetroPay.Reservation;

using Acme.Workforce.Payrollops.RetroPay.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AdaptiveRetroPayCheckpointChecks
{
    public static void AdaptiveRetroPayCheckpointStartsInCreated()
    {
        var item = AdaptiveRetroPayCheckpoint.NewAdaptiveRetroPayCheckpoint("code-194", "name-767");
        Check.Equal(item.Status, AdaptiveRetroPayCheckpointStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveRetroPayCheckpointMovesToPublished()
    {
        var item = AdaptiveRetroPayCheckpoint.NewAdaptiveRetroPayCheckpoint("code-194", "name-767");
        try
        {
            item.MoveTo(AdaptiveRetroPayCheckpointStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveRetroPayCheckpointStatus.Published, "status");
    }

    public static void AdaptiveRetroPayCheckpointRejectsMovesFromRetired()
    {
        var item = AdaptiveRetroPayCheckpoint.NewAdaptiveRetroPayCheckpoint("code-194", "name-767");
        item.Status = AdaptiveRetroPayCheckpointStatus.Retired;
        Check.Fails(() => item.MoveTo(AdaptiveRetroPayCheckpointStatus.Created), "move");
    }

    public static void RunAll()
    {
        AdaptiveRetroPayCheckpointStartsInCreated();
        AdaptiveRetroPayCheckpointMovesToPublished();
        AdaptiveRetroPayCheckpointRejectsMovesFromRetired();
    }
}
