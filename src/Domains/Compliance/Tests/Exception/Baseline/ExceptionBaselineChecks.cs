namespace Acme.Workforce.Compliance.Tests.Exception.Baseline;

using Acme.Workforce.Compliance.Exception.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ExceptionBaselineChecks
{
    public static void ExceptionBaselineStartsInOpen()
    {
        var item = ExceptionBaseline.NewExceptionBaseline("code-920", "label-873");
        Check.Equal(item.Status, ExceptionBaselineStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionBaselineMovesToInReview()
    {
        var item = ExceptionBaseline.NewExceptionBaseline("code-920", "label-873");
        try
        {
            item.MoveTo(ExceptionBaselineStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionBaselineStatus.InReview, "status");
    }

    public static void ExceptionBaselineRejectsMovesFromArchived()
    {
        var item = ExceptionBaseline.NewExceptionBaseline("code-920", "label-873");
        item.Status = ExceptionBaselineStatus.Archived;
        Check.Fails(() => item.MoveTo(ExceptionBaselineStatus.Open), "move");
    }

    public static void RunAll()
    {
        ExceptionBaselineStartsInOpen();
        ExceptionBaselineMovesToInReview();
        ExceptionBaselineRejectsMovesFromArchived();
    }
}
