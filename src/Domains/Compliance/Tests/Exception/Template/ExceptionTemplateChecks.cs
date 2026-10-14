namespace Acme.Workforce.Compliance.Tests.Exception.Template;

using Acme.Workforce.Compliance.Exception.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ExceptionTemplateChecks
{
    public static void ExceptionTemplateStartsInNew()
    {
        var item = ExceptionTemplate.NewExceptionTemplate("code-325", "title-116");
        Check.Equal(item.Status, ExceptionTemplateStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionTemplateMovesToDone()
    {
        var item = ExceptionTemplate.NewExceptionTemplate("code-325", "title-116");
        try
        {
            item.MoveTo(ExceptionTemplateStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionTemplateStatus.Done, "status");
    }

    public static void ExceptionTemplateRejectsMovesFromFailed()
    {
        var item = ExceptionTemplate.NewExceptionTemplate("code-325", "title-116");
        item.Status = ExceptionTemplateStatus.Failed;
        Check.Fails(() => item.MoveTo(ExceptionTemplateStatus.New), "move");
    }

    public static void RunAll()
    {
        ExceptionTemplateStartsInNew();
        ExceptionTemplateMovesToDone();
        ExceptionTemplateRejectsMovesFromFailed();
    }
}
