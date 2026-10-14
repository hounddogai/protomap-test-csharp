namespace Acme.Workforce.Payrollops.Tests.TaxForm.Proposal;

using Acme.Workforce.Payrollops.TaxForm.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class TaxFormNoticeChecks
{
    public static void TaxFormNoticeStartsInIdle()
    {
        var item = TaxFormNotice.NewTaxFormNotice("code-379", "label-166");
        Check.Equal(item.Status, TaxFormNoticeStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TaxFormNoticeMovesToWarming()
    {
        var item = TaxFormNotice.NewTaxFormNotice("code-379", "label-166");
        try
        {
            item.MoveTo(TaxFormNoticeStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TaxFormNoticeStatus.Warming, "status");
    }

    public static void TaxFormNoticeRejectsMovesFromStopped()
    {
        var item = TaxFormNotice.NewTaxFormNotice("code-379", "label-166");
        item.Status = TaxFormNoticeStatus.Stopped;
        Check.Fails(() => item.MoveTo(TaxFormNoticeStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TaxFormNoticeStartsInIdle();
        TaxFormNoticeMovesToWarming();
        TaxFormNoticeRejectsMovesFromStopped();
    }
}
