namespace Acme.Workforce.Workplace.Tests.Visitor.Entry;

using Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class DefaultVisitorTemplateChecks
{
    public static void DefaultVisitorTemplateStartsInPlanned()
    {
        var item = DefaultVisitorTemplate.NewDefaultVisitorTemplate("code-950", "name-559");
        Check.Equal(item.Status, DefaultVisitorTemplateStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultVisitorTemplateMovesToCancelled()
    {
        var item = DefaultVisitorTemplate.NewDefaultVisitorTemplate("code-950", "name-559");
        try
        {
            item.MoveTo(DefaultVisitorTemplateStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultVisitorTemplateStatus.Cancelled, "status");
    }

    public static void DefaultVisitorTemplateRejectsMovesFromCancelled()
    {
        var item = DefaultVisitorTemplate.NewDefaultVisitorTemplate("code-950", "name-559");
        item.Status = DefaultVisitorTemplateStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultVisitorTemplateStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultVisitorTemplateStartsInPlanned();
        DefaultVisitorTemplateMovesToCancelled();
        DefaultVisitorTemplateRejectsMovesFromCancelled();
    }
}
