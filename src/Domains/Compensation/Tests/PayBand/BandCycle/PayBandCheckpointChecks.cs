namespace Acme.Workforce.Compensation.Tests.PayBand.BandCycle;

using Acme.Workforce.Compensation.PayBand.BandCycle;

/// <summary>Checks of band cycle code, run by the project's runner.</summary>
public static class PayBandCheckpointChecks
{
    public static void PayBandCheckpointStartsInCreated()
    {
        var item = PayBandCheckpoint.NewPayBandCheckpoint("code-820", "name-806");
        Check.Equal(item.Status, PayBandCheckpointStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandCheckpointMovesToValidated()
    {
        var item = PayBandCheckpoint.NewPayBandCheckpoint("code-820", "name-806");
        try
        {
            item.MoveTo(PayBandCheckpointStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandCheckpointStatus.Validated, "status");
    }

    public static void PayBandCheckpointRejectsMovesFromRetired()
    {
        var item = PayBandCheckpoint.NewPayBandCheckpoint("code-820", "name-806");
        item.Status = PayBandCheckpointStatus.Retired;
        Check.Fails(() => item.MoveTo(PayBandCheckpointStatus.Created), "move");
    }

    public static void RunAll()
    {
        PayBandCheckpointStartsInCreated();
        PayBandCheckpointMovesToValidated();
        PayBandCheckpointRejectsMovesFromRetired();
    }
}
