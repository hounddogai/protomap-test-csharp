namespace Acme.Workforce.Workplace.Tests.Desk.Draft;

using Acme.Workforce.Workplace.Desk.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientDeskTemplateChecks
{
    public static void LenientDeskTemplateStartsInOpen()
    {
        var item = LenientDeskTemplate.NewLenientDeskTemplate("code-682", "name-157");
        Check.Equal(item.Status, LenientDeskTemplateStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientDeskTemplateMovesToResolved()
    {
        var item = LenientDeskTemplate.NewLenientDeskTemplate("code-682", "name-157");
        try
        {
            item.MoveTo(LenientDeskTemplateStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientDeskTemplateStatus.Resolved, "status");
    }

    public static void LenientDeskTemplateRejectsMovesFromArchived()
    {
        var item = LenientDeskTemplate.NewLenientDeskTemplate("code-682", "name-157");
        item.Status = LenientDeskTemplateStatus.Archived;
        Check.Fails(() => item.MoveTo(LenientDeskTemplateStatus.Open), "move");
    }

    public static void RunAll()
    {
        LenientDeskTemplateStartsInOpen();
        LenientDeskTemplateMovesToResolved();
        LenientDeskTemplateRejectsMovesFromArchived();
    }
}
