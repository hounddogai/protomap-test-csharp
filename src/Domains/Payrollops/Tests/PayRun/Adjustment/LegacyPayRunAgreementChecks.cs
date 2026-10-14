namespace Acme.Workforce.Payrollops.Tests.PayRun.Adjustment;

using Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LegacyPayRunAgreementChecks
{
    public static void LegacyPayRunAgreementStartsInRequested()
    {
        var item = LegacyPayRunAgreement.NewLegacyPayRunAgreement("code-833", "title-261");
        Check.Equal(item.Status, LegacyPayRunAgreementStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyPayRunAgreementMovesToGranted()
    {
        var item = LegacyPayRunAgreement.NewLegacyPayRunAgreement("code-833", "title-261");
        try
        {
            item.MoveTo(LegacyPayRunAgreementStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyPayRunAgreementStatus.Granted, "status");
    }

    public static void LegacyPayRunAgreementRejectsMovesFromExpired()
    {
        var item = LegacyPayRunAgreement.NewLegacyPayRunAgreement("code-833", "title-261");
        item.Status = LegacyPayRunAgreementStatus.Expired;
        Check.Fails(() => item.MoveTo(LegacyPayRunAgreementStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LegacyPayRunAgreementStartsInRequested();
        LegacyPayRunAgreementMovesToGranted();
        LegacyPayRunAgreementRejectsMovesFromExpired();
    }
}
