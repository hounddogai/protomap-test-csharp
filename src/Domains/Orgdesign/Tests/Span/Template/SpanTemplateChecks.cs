namespace Acme.Workforce.Orgdesign.Tests.Span.Template;

using Acme.Workforce.Orgdesign.Span.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class SpanTemplateChecks
{
    public static void SpanTemplateStartsInProposed()
    {
        var item = SpanTemplate.NewSpanTemplate("code-852", "label-134");
        Check.Equal(item.Status, SpanTemplateStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanTemplateMovesToRejected()
    {
        var item = SpanTemplate.NewSpanTemplate("code-852", "label-134");
        try
        {
            item.MoveTo(SpanTemplateStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanTemplateStatus.Rejected, "status");
    }

    public static void SpanTemplateRejectsMovesFromWithdrawn()
    {
        var item = SpanTemplate.NewSpanTemplate("code-852", "label-134");
        item.Status = SpanTemplateStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SpanTemplateStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SpanTemplateStartsInProposed();
        SpanTemplateMovesToRejected();
        SpanTemplateRejectsMovesFromWithdrawn();
    }
}
