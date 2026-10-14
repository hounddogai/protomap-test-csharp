namespace Acme.Workforce.Orgdesign.Tests.CostModel.Budget;

using Acme.Workforce.Orgdesign.CostModel.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class CostModelMappingChecks
{
    public static void CostModelMappingStartsInDraft()
    {
        var item = CostModelMapping.NewCostModelMapping("code-52", "name-606");
        Check.Equal(item.Status, CostModelMappingStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelMappingMovesToClosed()
    {
        var item = CostModelMapping.NewCostModelMapping("code-52", "name-606");
        try
        {
            item.MoveTo(CostModelMappingStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelMappingStatus.Closed, "status");
    }

    public static void CostModelMappingRejectsMovesFromClosed()
    {
        var item = CostModelMapping.NewCostModelMapping("code-52", "name-606");
        item.Status = CostModelMappingStatus.Closed;
        Check.Fails(() => item.MoveTo(CostModelMappingStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CostModelMappingStartsInDraft();
        CostModelMappingMovesToClosed();
        CostModelMappingRejectsMovesFromClosed();
    }
}
