namespace Acme.Workforce.Orgdesign.Tests.Span.Draft;

using Acme.Workforce.Orgdesign.Span.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class SpanAgreementChecks
{
    public static void SpanAgreementStartsInRequested()
    {
        var item = SpanAgreement.NewSpanAgreement("code-621", "name-257");
        Check.Equal(item.Status, SpanAgreementStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SpanAgreementMovesToGranted()
    {
        var item = SpanAgreement.NewSpanAgreement("code-621", "name-257");
        try
        {
            item.MoveTo(SpanAgreementStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SpanAgreementStatus.Granted, "status");
    }

    public static void SpanAgreementRejectsMovesFromExpired()
    {
        var item = SpanAgreement.NewSpanAgreement("code-621", "name-257");
        item.Status = SpanAgreementStatus.Expired;
        Check.Fails(() => item.MoveTo(SpanAgreementStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SpanAgreementStartsInRequested();
        SpanAgreementMovesToGranted();
        SpanAgreementRejectsMovesFromExpired();
    }
}
