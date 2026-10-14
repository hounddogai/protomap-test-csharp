namespace Acme.Workforce.Orgdesign.Tests.CostModel.Group;

using Acme.Workforce.Orgdesign.CostModel.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class CostModelTierChecks
{
    public static void CostModelTierStartsInOpen()
    {
        var item = CostModelTier.NewCostModelTier("code-995", "name-304");
        Check.Equal(item.Status, CostModelTierStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelTierMovesToArchived()
    {
        var item = CostModelTier.NewCostModelTier("code-995", "name-304");
        try
        {
            item.MoveTo(CostModelTierStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelTierStatus.Archived, "status");
    }

    public static void CostModelTierRejectsMovesFromArchived()
    {
        var item = CostModelTier.NewCostModelTier("code-995", "name-304");
        item.Status = CostModelTierStatus.Archived;
        Check.Fails(() => item.MoveTo(CostModelTierStatus.Open), "move");
    }

    public static void RunAll()
    {
        CostModelTierStartsInOpen();
        CostModelTierMovesToArchived();
        CostModelTierRejectsMovesFromArchived();
    }
}
