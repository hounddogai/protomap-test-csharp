namespace Acme.Workforce.Workplace.Tests.Occupancy.Adjustment;

using Acme.Workforce.Workplace.Occupancy.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class OccupancyRuleChecks
{
    public static void OccupancyRuleStartsInCreated()
    {
        var item = OccupancyRule.NewOccupancyRule("code-760", "title-177");
        Check.Equal(item.Status, OccupancyRuleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyRuleMovesToPublished()
    {
        var item = OccupancyRule.NewOccupancyRule("code-760", "title-177");
        try
        {
            item.MoveTo(OccupancyRuleStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyRuleStatus.Published, "status");
    }

    public static void OccupancyRuleRejectsMovesFromRetired()
    {
        var item = OccupancyRule.NewOccupancyRule("code-760", "title-177");
        item.Status = OccupancyRuleStatus.Retired;
        Check.Fails(() => item.MoveTo(OccupancyRuleStatus.Created), "move");
    }

    public static void RunAll()
    {
        OccupancyRuleStartsInCreated();
        OccupancyRuleMovesToPublished();
        OccupancyRuleRejectsMovesFromRetired();
    }
}
