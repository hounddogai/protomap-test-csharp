namespace Acme.Workforce.Payrollops.Tests.RetroPay.Review;

using Acme.Workforce.Payrollops.RetroPay.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RetroPayAgreementChecks
{
    public static void RetroPayAgreementStartsInPlanned()
    {
        var item = RetroPayAgreement.NewRetroPayAgreement("code-470", "name-498");
        Check.Equal(item.Status, RetroPayAgreementStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPayAgreementMovesToScheduled()
    {
        var item = RetroPayAgreement.NewRetroPayAgreement("code-470", "name-498");
        try
        {
            item.MoveTo(RetroPayAgreementStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPayAgreementStatus.Scheduled, "status");
    }

    public static void RetroPayAgreementRejectsMovesFromCancelled()
    {
        var item = RetroPayAgreement.NewRetroPayAgreement("code-470", "name-498");
        item.Status = RetroPayAgreementStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RetroPayAgreementStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RetroPayAgreementStartsInPlanned();
        RetroPayAgreementMovesToScheduled();
        RetroPayAgreementRejectsMovesFromCancelled();
    }
}
