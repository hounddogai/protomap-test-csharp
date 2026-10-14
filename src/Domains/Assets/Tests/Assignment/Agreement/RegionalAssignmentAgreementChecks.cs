namespace Acme.Workforce.Assets.Tests.Assignment.Agreement;

using Acme.Workforce.Assets.Assignment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class RegionalAssignmentAgreementChecks
{
    public static void RegionalAssignmentAgreementStartsInIdle()
    {
        var item = RegionalAssignmentAgreement.NewRegionalAssignmentAgreement("code-639", "name-487");
        Check.Equal(item.Status, RegionalAssignmentAgreementStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalAssignmentAgreementMovesToWarming()
    {
        var item = RegionalAssignmentAgreement.NewRegionalAssignmentAgreement("code-639", "name-487");
        try
        {
            item.MoveTo(RegionalAssignmentAgreementStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalAssignmentAgreementStatus.Warming, "status");
    }

    public static void RegionalAssignmentAgreementRejectsMovesFromStopped()
    {
        var item = RegionalAssignmentAgreement.NewRegionalAssignmentAgreement("code-639", "name-487");
        item.Status = RegionalAssignmentAgreementStatus.Stopped;
        Check.Fails(() => item.MoveTo(RegionalAssignmentAgreementStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RegionalAssignmentAgreementStartsInIdle();
        RegionalAssignmentAgreementMovesToWarming();
        RegionalAssignmentAgreementRejectsMovesFromStopped();
    }
}
