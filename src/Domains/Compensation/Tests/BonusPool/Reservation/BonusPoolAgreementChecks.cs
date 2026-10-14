namespace Acme.Workforce.Compensation.Tests.BonusPool.Reservation;

using Acme.Workforce.Compensation.BonusPool.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BonusPoolAgreementChecks
{
    public static void BonusPoolAgreementStartsInIdle()
    {
        var item = BonusPoolAgreement.NewBonusPoolAgreement("code-631", "title-974");
        Check.Equal(item.Status, BonusPoolAgreementStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolAgreementMovesToReady()
    {
        var item = BonusPoolAgreement.NewBonusPoolAgreement("code-631", "title-974");
        try
        {
            item.MoveTo(BonusPoolAgreementStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolAgreementStatus.Ready, "status");
    }

    public static void BonusPoolAgreementRejectsMovesFromStopped()
    {
        var item = BonusPoolAgreement.NewBonusPoolAgreement("code-631", "title-974");
        item.Status = BonusPoolAgreementStatus.Stopped;
        Check.Fails(() => item.MoveTo(BonusPoolAgreementStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BonusPoolAgreementStartsInIdle();
        BonusPoolAgreementMovesToReady();
        BonusPoolAgreementRejectsMovesFromStopped();
    }
}
