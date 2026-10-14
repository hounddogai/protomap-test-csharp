namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class CostModelVarianceChecks
{
    public static void CostModelVarianceStartsInDraft()
    {
        var item = CostModelVariance.NewCostModelVariance("code-213", "name-552");
        Check.Equal(item.Status, CostModelVarianceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelVarianceMovesToPending()
    {
        var item = CostModelVariance.NewCostModelVariance("code-213", "name-552");
        try
        {
            item.MoveTo(CostModelVarianceStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelVarianceStatus.Pending, "status");
    }

    public static void CostModelVarianceRejectsMovesFromClosed()
    {
        var item = CostModelVariance.NewCostModelVariance("code-213", "name-552");
        item.Status = CostModelVarianceStatus.Closed;
        Check.Fails(() => item.MoveTo(CostModelVarianceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        CostModelVarianceStartsInDraft();
        CostModelVarianceMovesToPending();
        CostModelVarianceRejectsMovesFromClosed();
    }
}
