namespace Acme.Workforce.Orgdesign.Tests.CostModel.Preference;

using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class CostModelGroupChecks
{
    public static void CostModelGroupStartsInOpen()
    {
        var item = CostModelGroup.NewCostModelGroup("code-404", "name-897");
        Check.Equal(item.Status, CostModelGroupStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelGroupMovesToInReview()
    {
        var item = CostModelGroup.NewCostModelGroup("code-404", "name-897");
        try
        {
            item.MoveTo(CostModelGroupStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelGroupStatus.InReview, "status");
    }

    public static void CostModelGroupRejectsMovesFromArchived()
    {
        var item = CostModelGroup.NewCostModelGroup("code-404", "name-897");
        item.Status = CostModelGroupStatus.Archived;
        Check.Fails(() => item.MoveTo(CostModelGroupStatus.Open), "move");
    }

    public static void RunAll()
    {
        CostModelGroupStartsInOpen();
        CostModelGroupMovesToInReview();
        CostModelGroupRejectsMovesFromArchived();
    }
}
