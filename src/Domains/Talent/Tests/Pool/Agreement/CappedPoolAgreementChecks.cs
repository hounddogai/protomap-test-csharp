namespace Acme.Workforce.Talent.Tests.Pool.Agreement;

using Acme.Workforce.Talent.Pool.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class CappedPoolAgreementChecks
{
    public static void CappedPoolAgreementStartsInNew()
    {
        var item = CappedPoolAgreement.NewCappedPoolAgreement("code-173", "title-59");
        Check.Equal(item.Status, CappedPoolAgreementStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedPoolAgreementMovesToQueued()
    {
        var item = CappedPoolAgreement.NewCappedPoolAgreement("code-173", "title-59");
        try
        {
            item.MoveTo(CappedPoolAgreementStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedPoolAgreementStatus.Queued, "status");
    }

    public static void CappedPoolAgreementRejectsMovesFromFailed()
    {
        var item = CappedPoolAgreement.NewCappedPoolAgreement("code-173", "title-59");
        item.Status = CappedPoolAgreementStatus.Failed;
        Check.Fails(() => item.MoveTo(CappedPoolAgreementStatus.New), "move");
    }

    public static void RunAll()
    {
        CappedPoolAgreementStartsInNew();
        CappedPoolAgreementMovesToQueued();
        CappedPoolAgreementRejectsMovesFromFailed();
    }
}
