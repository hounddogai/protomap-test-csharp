namespace Acme.Workforce.Compensation.Tests.EquityGrant.Group;

using Acme.Workforce.Compensation.EquityGrant.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class EquityGrantGroupChecks
{
    public static void EquityGrantGroupStartsInNew()
    {
        var item = EquityGrantGroup.NewEquityGrantGroup("code-764", "label-775");
        Check.Equal(item.Status, EquityGrantGroupStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantGroupMovesToRunning()
    {
        var item = EquityGrantGroup.NewEquityGrantGroup("code-764", "label-775");
        try
        {
            item.MoveTo(EquityGrantGroupStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantGroupStatus.Running, "status");
    }

    public static void EquityGrantGroupRejectsMovesFromFailed()
    {
        var item = EquityGrantGroup.NewEquityGrantGroup("code-764", "label-775");
        item.Status = EquityGrantGroupStatus.Failed;
        Check.Fails(() => item.MoveTo(EquityGrantGroupStatus.New), "move");
    }

    public static void RunAll()
    {
        EquityGrantGroupStartsInNew();
        EquityGrantGroupMovesToRunning();
        EquityGrantGroupRejectsMovesFromFailed();
    }
}
