namespace Acme.Workforce.Absence.Tests.LeaveRequest.Digest;

using Acme.Workforce.Absence.LeaveRequest.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class LeaveRequestAgreementChecks
{
    public static void LeaveRequestAgreementStartsInPlanned()
    {
        var item = LeaveRequestAgreement.NewLeaveRequestAgreement("code-352", "name-842");
        Check.Equal(item.Status, LeaveRequestAgreementStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestAgreementMovesToScheduled()
    {
        var item = LeaveRequestAgreement.NewLeaveRequestAgreement("code-352", "name-842");
        try
        {
            item.MoveTo(LeaveRequestAgreementStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestAgreementStatus.Scheduled, "status");
    }

    public static void LeaveRequestAgreementRejectsMovesFromCancelled()
    {
        var item = LeaveRequestAgreement.NewLeaveRequestAgreement("code-352", "name-842");
        item.Status = LeaveRequestAgreementStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LeaveRequestAgreementStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LeaveRequestAgreementStartsInPlanned();
        LeaveRequestAgreementMovesToScheduled();
        LeaveRequestAgreementRejectsMovesFromCancelled();
    }
}
