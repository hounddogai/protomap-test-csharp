namespace Acme.Workforce.Workplace.Tests.Visitor.Group;

using Acme.Workforce.Workplace.Visitor.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class VisitorProfileChecks
{
    public static void VisitorProfileStartsInNew()
    {
        var item = VisitorProfile.NewVisitorProfile("code-942", "label-876");
        Check.Equal(item.Status, VisitorProfileStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorProfileMovesToRunning()
    {
        var item = VisitorProfile.NewVisitorProfile("code-942", "label-876");
        try
        {
            item.MoveTo(VisitorProfileStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorProfileStatus.Running, "status");
    }

    public static void VisitorProfileRejectsMovesFromFailed()
    {
        var item = VisitorProfile.NewVisitorProfile("code-942", "label-876");
        item.Status = VisitorProfileStatus.Failed;
        Check.Fails(() => item.MoveTo(VisitorProfileStatus.New), "move");
    }

    public static void RunAll()
    {
        VisitorProfileStartsInNew();
        VisitorProfileMovesToRunning();
        VisitorProfileRejectsMovesFromFailed();
    }
}
