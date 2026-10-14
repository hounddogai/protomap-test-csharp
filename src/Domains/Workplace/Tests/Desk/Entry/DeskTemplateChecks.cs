namespace Acme.Workforce.Workplace.Tests.Desk.Entry;

using Acme.Workforce.Workplace.Desk.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class DeskTemplateChecks
{
    public static void DeskTemplateStartsInOpen()
    {
        var item = DeskTemplate.NewDeskTemplate("code-183", "label-155");
        Check.Equal(item.Status, DeskTemplateStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeskTemplateMovesToArchived()
    {
        var item = DeskTemplate.NewDeskTemplate("code-183", "label-155");
        try
        {
            item.MoveTo(DeskTemplateStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeskTemplateStatus.Archived, "status");
    }

    public static void DeskTemplateRejectsMovesFromArchived()
    {
        var item = DeskTemplate.NewDeskTemplate("code-183", "label-155");
        item.Status = DeskTemplateStatus.Archived;
        Check.Fails(() => item.MoveTo(DeskTemplateStatus.Open), "move");
    }

    public static void RunAll()
    {
        DeskTemplateStartsInOpen();
        DeskTemplateMovesToArchived();
        DeskTemplateRejectsMovesFromArchived();
    }
}
