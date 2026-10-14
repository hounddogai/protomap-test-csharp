namespace Acme.Workforce.Compensation.Tests.PayBand.Summary;

using Acme.Workforce.Compensation.PayBand.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PayBandSummaryChecks
{
    public static void PayBandSummaryStartsInRequested()
    {
        var item = PayBandSummary.NewPayBandSummary("code-18", "title-11");
        Check.Equal(item.Status, PayBandSummaryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandSummaryMovesToRevoked()
    {
        var item = PayBandSummary.NewPayBandSummary("code-18", "title-11");
        try
        {
            item.MoveTo(PayBandSummaryStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandSummaryStatus.Revoked, "status");
    }

    public static void PayBandSummaryRejectsMovesFromExpired()
    {
        var item = PayBandSummary.NewPayBandSummary("code-18", "title-11");
        item.Status = PayBandSummaryStatus.Expired;
        Check.Fails(() => item.MoveTo(PayBandSummaryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PayBandSummaryStartsInRequested();
        PayBandSummaryMovesToRevoked();
        PayBandSummaryRejectsMovesFromExpired();
    }
}
