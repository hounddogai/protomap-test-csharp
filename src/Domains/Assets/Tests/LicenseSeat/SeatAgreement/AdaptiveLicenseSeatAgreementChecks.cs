namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAgreement;

using Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

/// <summary>Checks of seat agreement code, run by the project's runner.</summary>
public static class AdaptiveLicenseSeatAgreementChecks
{
    public static void AdaptiveLicenseSeatAgreementStartsInIdle()
    {
        var item = AdaptiveLicenseSeatAgreement.NewAdaptiveLicenseSeatAgreement("code-384", "label-132");
        Check.Equal(item.Status, AdaptiveLicenseSeatAgreementStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveLicenseSeatAgreementMovesToStopped()
    {
        var item = AdaptiveLicenseSeatAgreement.NewAdaptiveLicenseSeatAgreement("code-384", "label-132");
        try
        {
            item.MoveTo(AdaptiveLicenseSeatAgreementStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveLicenseSeatAgreementStatus.Stopped, "status");
    }

    public static void AdaptiveLicenseSeatAgreementRejectsMovesFromStopped()
    {
        var item = AdaptiveLicenseSeatAgreement.NewAdaptiveLicenseSeatAgreement("code-384", "label-132");
        item.Status = AdaptiveLicenseSeatAgreementStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdaptiveLicenseSeatAgreementStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdaptiveLicenseSeatAgreementStartsInIdle();
        AdaptiveLicenseSeatAgreementMovesToStopped();
        AdaptiveLicenseSeatAgreementRejectsMovesFromStopped();
    }
}
