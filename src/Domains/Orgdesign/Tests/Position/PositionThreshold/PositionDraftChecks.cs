namespace Acme.Workforce.Orgdesign.Tests.Position.PositionThreshold;

using Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>Checks of position threshold code, run by the project's runner.</summary>
public static class PositionDraftChecks
{
    public static void PositionDraftStartsInOpen()
    {
        var item = PositionDraft.NewPositionDraft("code-864", "name-359");
        Check.Equal(item.Status, PositionDraftStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionDraftMovesToResolved()
    {
        var item = PositionDraft.NewPositionDraft("code-864", "name-359");
        try
        {
            item.MoveTo(PositionDraftStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionDraftStatus.Resolved, "status");
    }

    public static void PositionDraftRejectsMovesFromArchived()
    {
        var item = PositionDraft.NewPositionDraft("code-864", "name-359");
        item.Status = PositionDraftStatus.Archived;
        Check.Fails(() => item.MoveTo(PositionDraftStatus.Open), "move");
    }

    public static void RunAll()
    {
        PositionDraftStartsInOpen();
        PositionDraftMovesToResolved();
        PositionDraftRejectsMovesFromArchived();
    }
}
