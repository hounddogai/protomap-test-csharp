namespace Acme.Workforce.Orgdesign.Tests.CostModel.Policy;

using Acme.Workforce.Orgdesign.CostModel.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class CostModelExceptionChecks
{
    public static void CostModelExceptionStartsInNew()
    {
        var item = CostModelException.NewCostModelException("code-565", "name-417");
        Check.Equal(item.Status, CostModelExceptionStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelExceptionMovesToRunning()
    {
        var item = CostModelException.NewCostModelException("code-565", "name-417");
        try
        {
            item.MoveTo(CostModelExceptionStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelExceptionStatus.Running, "status");
    }

    public static void CostModelExceptionRejectsMovesFromFailed()
    {
        var item = CostModelException.NewCostModelException("code-565", "name-417");
        item.Status = CostModelExceptionStatus.Failed;
        Check.Fails(() => item.MoveTo(CostModelExceptionStatus.New), "move");
    }

    public static void RunAll()
    {
        CostModelExceptionStartsInNew();
        CostModelExceptionMovesToRunning();
        CostModelExceptionRejectsMovesFromFailed();
    }
}
