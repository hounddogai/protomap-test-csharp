namespace Acme.Workforce.Workplace.Tests.Visitor.Exception;

using Acme.Workforce.Workplace.Visitor.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class LenientVisitorExceptionChecks
{
    public static void LenientVisitorExceptionStartsInOpen()
    {
        var item = LenientVisitorException.NewLenientVisitorException("code-439", "name-453");
        Check.Equal(item.Status, LenientVisitorExceptionStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientVisitorExceptionMovesToInReview()
    {
        var item = LenientVisitorException.NewLenientVisitorException("code-439", "name-453");
        try
        {
            item.MoveTo(LenientVisitorExceptionStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientVisitorExceptionStatus.InReview, "status");
    }

    public static void LenientVisitorExceptionRejectsMovesFromArchived()
    {
        var item = LenientVisitorException.NewLenientVisitorException("code-439", "name-453");
        item.Status = LenientVisitorExceptionStatus.Archived;
        Check.Fails(() => item.MoveTo(LenientVisitorExceptionStatus.Open), "move");
    }

    public static void RunAll()
    {
        LenientVisitorExceptionStartsInOpen();
        LenientVisitorExceptionMovesToInReview();
        LenientVisitorExceptionRejectsMovesFromArchived();
    }
}
