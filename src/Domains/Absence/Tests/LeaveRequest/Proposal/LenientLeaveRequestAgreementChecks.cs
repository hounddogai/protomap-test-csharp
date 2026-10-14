namespace Acme.Workforce.Absence.Tests.LeaveRequest.Proposal;

using Acme.Workforce.Absence.LeaveRequest.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class LenientLeaveRequestAgreementChecks
{
    public static void LenientLeaveRequestAgreementStartsInDraft()
    {
        var item = LenientLeaveRequestAgreement.NewLenientLeaveRequestAgreement("code-193", "name-871");
        Check.Equal(item.Status, LenientLeaveRequestAgreementStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientLeaveRequestAgreementMovesToActive()
    {
        var item = LenientLeaveRequestAgreement.NewLenientLeaveRequestAgreement("code-193", "name-871");
        try
        {
            item.MoveTo(LenientLeaveRequestAgreementStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientLeaveRequestAgreementStatus.Active, "status");
    }

    public static void LenientLeaveRequestAgreementRejectsMovesFromClosed()
    {
        var item = LenientLeaveRequestAgreement.NewLenientLeaveRequestAgreement("code-193", "name-871");
        item.Status = LenientLeaveRequestAgreementStatus.Closed;
        Check.Fails(() => item.MoveTo(LenientLeaveRequestAgreementStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LenientLeaveRequestAgreementStartsInDraft();
        LenientLeaveRequestAgreementMovesToActive();
        LenientLeaveRequestAgreementRejectsMovesFromClosed();
    }
}
