namespace Acme.Workforce.Orgdesign.Tests.Headcount.Group;

using Acme.Workforce.Orgdesign.Headcount.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class DefaultHeadcountProfileChecks
{
    public static void DefaultHeadcountProfileStartsInOpen()
    {
        var item = DefaultHeadcountProfile.NewDefaultHeadcountProfile("code-419", "label-523");
        Check.Equal(item.Status, DefaultHeadcountProfileStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultHeadcountProfileMovesToResolved()
    {
        var item = DefaultHeadcountProfile.NewDefaultHeadcountProfile("code-419", "label-523");
        try
        {
            item.MoveTo(DefaultHeadcountProfileStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultHeadcountProfileStatus.Resolved, "status");
    }

    public static void DefaultHeadcountProfileRejectsMovesFromArchived()
    {
        var item = DefaultHeadcountProfile.NewDefaultHeadcountProfile("code-419", "label-523");
        item.Status = DefaultHeadcountProfileStatus.Archived;
        Check.Fails(() => item.MoveTo(DefaultHeadcountProfileStatus.Open), "move");
    }

    public static void RunAll()
    {
        DefaultHeadcountProfileStartsInOpen();
        DefaultHeadcountProfileMovesToResolved();
        DefaultHeadcountProfileRejectsMovesFromArchived();
    }
}
