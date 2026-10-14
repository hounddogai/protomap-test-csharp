namespace Acme.Workforce.Contractors.Tests.Engagement.Assignment;

using Acme.Workforce.Contractors.Engagement.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class EngagementAgreementChecks
{
    public static void EngagementAgreementStartsInNew()
    {
        var item = EngagementAgreement.NewEngagementAgreement("code-580", "label-439");
        Check.Equal(item.Status, EngagementAgreementStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EngagementAgreementMovesToRunning()
    {
        var item = EngagementAgreement.NewEngagementAgreement("code-580", "label-439");
        try
        {
            item.MoveTo(EngagementAgreementStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EngagementAgreementStatus.Running, "status");
    }

    public static void EngagementAgreementRejectsMovesFromFailed()
    {
        var item = EngagementAgreement.NewEngagementAgreement("code-580", "label-439");
        item.Status = EngagementAgreementStatus.Failed;
        Check.Fails(() => item.MoveTo(EngagementAgreementStatus.New), "move");
    }

    public static void RunAll()
    {
        EngagementAgreementStartsInNew();
        EngagementAgreementMovesToRunning();
        EngagementAgreementRejectsMovesFromFailed();
    }
}
