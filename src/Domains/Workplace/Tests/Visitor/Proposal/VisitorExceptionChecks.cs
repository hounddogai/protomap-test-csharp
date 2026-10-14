namespace Acme.Workforce.Workplace.Tests.Visitor.Proposal;

using Acme.Workforce.Workplace.Visitor.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class VisitorExceptionChecks
{
    public static void VisitorExceptionStartsInCreated()
    {
        var item = VisitorException.NewVisitorException("code-826", "title-297");
        Check.Equal(item.Status, VisitorExceptionStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorExceptionMovesToPublished()
    {
        var item = VisitorException.NewVisitorException("code-826", "title-297");
        try
        {
            item.MoveTo(VisitorExceptionStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorExceptionStatus.Published, "status");
    }

    public static void VisitorExceptionRejectsMovesFromRetired()
    {
        var item = VisitorException.NewVisitorException("code-826", "title-297");
        item.Status = VisitorExceptionStatus.Retired;
        Check.Fails(() => item.MoveTo(VisitorExceptionStatus.Created), "move");
    }

    public static void RunAll()
    {
        VisitorExceptionStartsInCreated();
        VisitorExceptionMovesToPublished();
        VisitorExceptionRejectsMovesFromRetired();
    }
}
