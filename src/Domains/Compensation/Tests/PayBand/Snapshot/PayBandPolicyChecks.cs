namespace Acme.Workforce.Compensation.Tests.PayBand.Snapshot;

using Acme.Workforce.Compensation.PayBand.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class PayBandPolicyChecks
{
    public static void PayBandPolicyStartsInPlanned()
    {
        var item = PayBandPolicy.NewPayBandPolicy("code-608", "title-306");
        Check.Equal(item.Status, PayBandPolicyStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandPolicyMovesToScheduled()
    {
        var item = PayBandPolicy.NewPayBandPolicy("code-608", "title-306");
        try
        {
            item.MoveTo(PayBandPolicyStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandPolicyStatus.Scheduled, "status");
    }

    public static void PayBandPolicyRejectsMovesFromCancelled()
    {
        var item = PayBandPolicy.NewPayBandPolicy("code-608", "title-306");
        item.Status = PayBandPolicyStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PayBandPolicyStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PayBandPolicyStartsInPlanned();
        PayBandPolicyMovesToScheduled();
        PayBandPolicyRejectsMovesFromCancelled();
    }
}
