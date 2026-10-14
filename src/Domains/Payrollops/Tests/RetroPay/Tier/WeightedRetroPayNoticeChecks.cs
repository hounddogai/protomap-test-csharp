namespace Acme.Workforce.Payrollops.Tests.RetroPay.Tier;

using Acme.Workforce.Payrollops.RetroPay.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class WeightedRetroPayNoticeChecks
{
    public static void WeightedRetroPayNoticeStartsInProposed()
    {
        var item = WeightedRetroPayNotice.NewWeightedRetroPayNotice("code-201", "label-723");
        Check.Equal(item.Status, WeightedRetroPayNoticeStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedRetroPayNoticeMovesToWithdrawn()
    {
        var item = WeightedRetroPayNotice.NewWeightedRetroPayNotice("code-201", "label-723");
        try
        {
            item.MoveTo(WeightedRetroPayNoticeStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedRetroPayNoticeStatus.Withdrawn, "status");
    }

    public static void WeightedRetroPayNoticeRejectsMovesFromWithdrawn()
    {
        var item = WeightedRetroPayNotice.NewWeightedRetroPayNotice("code-201", "label-723");
        item.Status = WeightedRetroPayNoticeStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(WeightedRetroPayNoticeStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        WeightedRetroPayNoticeStartsInProposed();
        WeightedRetroPayNoticeMovesToWithdrawn();
        WeightedRetroPayNoticeRejectsMovesFromWithdrawn();
    }
}
