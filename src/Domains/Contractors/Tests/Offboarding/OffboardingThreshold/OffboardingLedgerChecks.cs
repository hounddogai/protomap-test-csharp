namespace Acme.Workforce.Contractors.Tests.Offboarding.OffboardingThreshold;

using Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

/// <summary>Checks of offboarding threshold code, run by the project's runner.</summary>
public static class OffboardingLedgerChecks
{
    public static void OffboardingLedgerStartsInOpen()
    {
        var item = OffboardingLedger.NewOffboardingLedger("code-807", "title-411");
        Check.Equal(item.Status, OffboardingLedgerStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OffboardingLedgerMovesToInReview()
    {
        var item = OffboardingLedger.NewOffboardingLedger("code-807", "title-411");
        try
        {
            item.MoveTo(OffboardingLedgerStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OffboardingLedgerStatus.InReview, "status");
    }

    public static void OffboardingLedgerRejectsMovesFromArchived()
    {
        var item = OffboardingLedger.NewOffboardingLedger("code-807", "title-411");
        item.Status = OffboardingLedgerStatus.Archived;
        Check.Fails(() => item.MoveTo(OffboardingLedgerStatus.Open), "move");
    }

    public static void RunAll()
    {
        OffboardingLedgerStartsInOpen();
        OffboardingLedgerMovesToInReview();
        OffboardingLedgerRejectsMovesFromArchived();
    }
}
