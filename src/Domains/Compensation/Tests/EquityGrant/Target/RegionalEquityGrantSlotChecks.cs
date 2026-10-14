namespace Acme.Workforce.Compensation.Tests.EquityGrant.Target;

using Acme.Workforce.Compensation.EquityGrant.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class RegionalEquityGrantSlotChecks
{
    public static void RegionalEquityGrantSlotStartsInOpen()
    {
        var item = RegionalEquityGrantSlot.NewRegionalEquityGrantSlot("code-449", "label-115");
        Check.Equal(item.Status, RegionalEquityGrantSlotStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalEquityGrantSlotMovesToInReview()
    {
        var item = RegionalEquityGrantSlot.NewRegionalEquityGrantSlot("code-449", "label-115");
        try
        {
            item.MoveTo(RegionalEquityGrantSlotStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalEquityGrantSlotStatus.InReview, "status");
    }

    public static void RegionalEquityGrantSlotRejectsMovesFromArchived()
    {
        var item = RegionalEquityGrantSlot.NewRegionalEquityGrantSlot("code-449", "label-115");
        item.Status = RegionalEquityGrantSlotStatus.Archived;
        Check.Fails(() => item.MoveTo(RegionalEquityGrantSlotStatus.Open), "move");
    }

    public static void RunAll()
    {
        RegionalEquityGrantSlotStartsInOpen();
        RegionalEquityGrantSlotMovesToInReview();
        RegionalEquityGrantSlotRejectsMovesFromArchived();
    }
}
