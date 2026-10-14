namespace Acme.Workforce.Compliance.Tests.Exception.Template;

using Acme.Workforce.Compliance.Exception.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class LenientExceptionVarianceChecks
{
    public static void LenientExceptionVarianceStartsInDraft()
    {
        var item = LenientExceptionVariance.NewLenientExceptionVariance("code-37", "label-18");
        Check.Equal(item.Status, LenientExceptionVarianceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientExceptionVarianceMovesToClosed()
    {
        var item = LenientExceptionVariance.NewLenientExceptionVariance("code-37", "label-18");
        try
        {
            item.MoveTo(LenientExceptionVarianceStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientExceptionVarianceStatus.Closed, "status");
    }

    public static void LenientExceptionVarianceRejectsMovesFromClosed()
    {
        var item = LenientExceptionVariance.NewLenientExceptionVariance("code-37", "label-18");
        item.Status = LenientExceptionVarianceStatus.Closed;
        Check.Fails(() => item.MoveTo(LenientExceptionVarianceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LenientExceptionVarianceStartsInDraft();
        LenientExceptionVarianceMovesToClosed();
        LenientExceptionVarianceRejectsMovesFromClosed();
    }
}
