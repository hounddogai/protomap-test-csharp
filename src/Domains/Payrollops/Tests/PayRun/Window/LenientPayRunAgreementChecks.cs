namespace Acme.Workforce.Payrollops.Tests.PayRun.Window;

using Acme.Workforce.Payrollops.PayRun.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class LenientPayRunAgreementChecks
{
    public static void LenientPayRunAgreementStartsInDraft()
    {
        var item = LenientPayRunAgreement.NewLenientPayRunAgreement("code-598", "title-119");
        Check.Equal(item.Status, LenientPayRunAgreementStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientPayRunAgreementMovesToActive()
    {
        var item = LenientPayRunAgreement.NewLenientPayRunAgreement("code-598", "title-119");
        try
        {
            item.MoveTo(LenientPayRunAgreementStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientPayRunAgreementStatus.Active, "status");
    }

    public static void LenientPayRunAgreementRejectsMovesFromClosed()
    {
        var item = LenientPayRunAgreement.NewLenientPayRunAgreement("code-598", "title-119");
        item.Status = LenientPayRunAgreementStatus.Closed;
        Check.Fails(() => item.MoveTo(LenientPayRunAgreementStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LenientPayRunAgreementStartsInDraft();
        LenientPayRunAgreementMovesToActive();
        LenientPayRunAgreementRejectsMovesFromClosed();
    }
}
