namespace Acme.Workforce.Workplace.Tests.Visitor.Assignment;

using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VisitorTemplateChecks
{
    public static void VisitorTemplateStartsInRequested()
    {
        var item = VisitorTemplate.NewVisitorTemplate("code-381", "name-824");
        Check.Equal(item.Status, VisitorTemplateStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void VisitorTemplateMovesToRevoked()
    {
        var item = VisitorTemplate.NewVisitorTemplate("code-381", "name-824");
        try
        {
            item.MoveTo(VisitorTemplateStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, VisitorTemplateStatus.Revoked, "status");
    }

    public static void VisitorTemplateRejectsMovesFromExpired()
    {
        var item = VisitorTemplate.NewVisitorTemplate("code-381", "name-824");
        item.Status = VisitorTemplateStatus.Expired;
        Check.Fails(() => item.MoveTo(VisitorTemplateStatus.Requested), "move");
    }

    public static void RunAll()
    {
        VisitorTemplateStartsInRequested();
        VisitorTemplateMovesToRevoked();
        VisitorTemplateRejectsMovesFromExpired();
    }
}
