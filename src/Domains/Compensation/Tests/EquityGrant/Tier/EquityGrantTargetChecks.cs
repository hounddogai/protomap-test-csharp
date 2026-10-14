namespace Acme.Workforce.Compensation.Tests.EquityGrant.Tier;

using Acme.Workforce.Compensation.EquityGrant.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class EquityGrantTargetChecks
{
    public static void EquityGrantTargetStartsInOpen()
    {
        var item = EquityGrantTarget.NewEquityGrantTarget("code-930", "label-40");
        Check.Equal(item.Status, EquityGrantTargetStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantTargetMovesToInReview()
    {
        var item = EquityGrantTarget.NewEquityGrantTarget("code-930", "label-40");
        try
        {
            item.MoveTo(EquityGrantTargetStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantTargetStatus.InReview, "status");
    }

    public static void EquityGrantTargetRejectsMovesFromArchived()
    {
        var item = EquityGrantTarget.NewEquityGrantTarget("code-930", "label-40");
        item.Status = EquityGrantTargetStatus.Archived;
        Check.Fails(() => item.MoveTo(EquityGrantTargetStatus.Open), "move");
    }

    public static void RunAll()
    {
        EquityGrantTargetStartsInOpen();
        EquityGrantTargetMovesToInReview();
        EquityGrantTargetRejectsMovesFromArchived();
    }
}
