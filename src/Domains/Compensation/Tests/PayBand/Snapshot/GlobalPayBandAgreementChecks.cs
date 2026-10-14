namespace Acme.Workforce.Compensation.Tests.PayBand.Snapshot;

using Acme.Workforce.Compensation.PayBand.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class GlobalPayBandAgreementChecks
{
    public static void GlobalPayBandAgreementStartsInPlanned()
    {
        var item = GlobalPayBandAgreement.NewGlobalPayBandAgreement("code-32", "label-144");
        Check.Equal(item.Status, GlobalPayBandAgreementStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalPayBandAgreementMovesToCancelled()
    {
        var item = GlobalPayBandAgreement.NewGlobalPayBandAgreement("code-32", "label-144");
        try
        {
            item.MoveTo(GlobalPayBandAgreementStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalPayBandAgreementStatus.Cancelled, "status");
    }

    public static void GlobalPayBandAgreementRejectsMovesFromCancelled()
    {
        var item = GlobalPayBandAgreement.NewGlobalPayBandAgreement("code-32", "label-144");
        item.Status = GlobalPayBandAgreementStatus.Cancelled;
        Check.Fails(() => item.MoveTo(GlobalPayBandAgreementStatus.Planned), "move");
    }

    public static void RunAll()
    {
        GlobalPayBandAgreementStartsInPlanned();
        GlobalPayBandAgreementMovesToCancelled();
        GlobalPayBandAgreementRejectsMovesFromCancelled();
    }
}
