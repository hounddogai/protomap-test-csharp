namespace Acme.Workforce.Orgdesign.Tests.Headcount.Agreement;

using Acme.Workforce.Orgdesign.Headcount.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class HeadcountAgreementChecks
{
    public static void HeadcountAgreementStartsInIdle()
    {
        var item = HeadcountAgreement.NewHeadcountAgreement("code-372", "label-801");
        Check.Equal(item.Status, HeadcountAgreementStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountAgreementMovesToDraining()
    {
        var item = HeadcountAgreement.NewHeadcountAgreement("code-372", "label-801");
        try
        {
            item.MoveTo(HeadcountAgreementStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountAgreementStatus.Draining, "status");
    }

    public static void HeadcountAgreementRejectsMovesFromStopped()
    {
        var item = HeadcountAgreement.NewHeadcountAgreement("code-372", "label-801");
        item.Status = HeadcountAgreementStatus.Stopped;
        Check.Fails(() => item.MoveTo(HeadcountAgreementStatus.Idle), "move");
    }

    public static void RunAll()
    {
        HeadcountAgreementStartsInIdle();
        HeadcountAgreementMovesToDraining();
        HeadcountAgreementRejectsMovesFromStopped();
    }
}
