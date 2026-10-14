namespace Acme.Workforce.Workplace.Tests.Occupancy.Rule;

using Acme.Workforce.Workplace.Occupancy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TieredOccupancyRuleChecks
{
    public static void TieredOccupancyRuleStartsInCreated()
    {
        var item = TieredOccupancyRule.NewTieredOccupancyRule("code-702", "label-675");
        Check.Equal(item.Status, TieredOccupancyRuleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredOccupancyRuleMovesToValidated()
    {
        var item = TieredOccupancyRule.NewTieredOccupancyRule("code-702", "label-675");
        try
        {
            item.MoveTo(TieredOccupancyRuleStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredOccupancyRuleStatus.Validated, "status");
    }

    public static void TieredOccupancyRuleRejectsMovesFromRetired()
    {
        var item = TieredOccupancyRule.NewTieredOccupancyRule("code-702", "label-675");
        item.Status = TieredOccupancyRuleStatus.Retired;
        Check.Fails(() => item.MoveTo(TieredOccupancyRuleStatus.Created), "move");
    }

    public static void RunAll()
    {
        TieredOccupancyRuleStartsInCreated();
        TieredOccupancyRuleMovesToValidated();
        TieredOccupancyRuleRejectsMovesFromRetired();
    }
}
