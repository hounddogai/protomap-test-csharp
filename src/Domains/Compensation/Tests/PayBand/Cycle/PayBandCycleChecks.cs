namespace Acme.Workforce.Compensation.Tests.PayBand.Cycle;

using Acme.Workforce.Compensation.PayBand.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PayBandCycleChecks
{
    public static void PayBandCycleStartsInNew()
    {
        var item = PayBandCycle.NewPayBandCycle("code-740", "name-954");
        Check.Equal(item.Status, PayBandCycleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayBandCycleMovesToDone()
    {
        var item = PayBandCycle.NewPayBandCycle("code-740", "name-954");
        try
        {
            item.MoveTo(PayBandCycleStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayBandCycleStatus.Done, "status");
    }

    public static void PayBandCycleRejectsMovesFromFailed()
    {
        var item = PayBandCycle.NewPayBandCycle("code-740", "name-954");
        item.Status = PayBandCycleStatus.Failed;
        Check.Fails(() => item.MoveTo(PayBandCycleStatus.New), "move");
    }

    public static void RunAll()
    {
        PayBandCycleStartsInNew();
        PayBandCycleMovesToDone();
        PayBandCycleRejectsMovesFromFailed();
    }
}
