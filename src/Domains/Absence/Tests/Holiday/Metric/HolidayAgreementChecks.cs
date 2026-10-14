namespace Acme.Workforce.Absence.Tests.Holiday.Metric;

using Acme.Workforce.Absence.Holiday.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class HolidayAgreementChecks
{
    public static void HolidayAgreementStartsInProposed()
    {
        var item = HolidayAgreement.NewHolidayAgreement("code-290", "name-447");
        Check.Equal(item.Status, HolidayAgreementStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayAgreementMovesToApproved()
    {
        var item = HolidayAgreement.NewHolidayAgreement("code-290", "name-447");
        try
        {
            item.MoveTo(HolidayAgreementStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayAgreementStatus.Approved, "status");
    }

    public static void HolidayAgreementRejectsMovesFromWithdrawn()
    {
        var item = HolidayAgreement.NewHolidayAgreement("code-290", "name-447");
        item.Status = HolidayAgreementStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(HolidayAgreementStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        HolidayAgreementStartsInProposed();
        HolidayAgreementMovesToApproved();
        HolidayAgreementRejectsMovesFromWithdrawn();
    }
}
