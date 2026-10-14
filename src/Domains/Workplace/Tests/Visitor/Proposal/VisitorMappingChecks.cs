namespace Acme.Workforce.Workplace.Tests.Visitor.Proposal;

using Acme.Workforce.Workplace.Visitor.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class VisitorMappingChecks
{
    public static void VisitorMappingStartsInOpen()
    {
        var item = VisitorMapping.NewVisitorMapping("code-364", "name-304");
        Check.Equal(item.Status, VisitorMappingStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorMappingMovesToArchived()
    {
        var item = VisitorMapping.NewVisitorMapping("code-364", "name-304");
        try
        {
            item.MoveTo(VisitorMappingStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorMappingStatus.Archived, "status");
    }

    public static void VisitorMappingRejectsMovesFromArchived()
    {
        var item = VisitorMapping.NewVisitorMapping("code-364", "name-304");
        item.Status = VisitorMappingStatus.Archived;
        Check.Fails(() => item.MoveTo(VisitorMappingStatus.Open), "move");
    }

    public static void RunAll()
    {
        VisitorMappingStartsInOpen();
        VisitorMappingMovesToArchived();
        VisitorMappingRejectsMovesFromArchived();
    }
}
